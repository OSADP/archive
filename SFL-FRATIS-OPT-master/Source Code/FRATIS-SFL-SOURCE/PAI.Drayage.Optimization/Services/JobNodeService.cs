using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization.Services
{
    public interface IJobNodeService
    {
        JobNode CreateJobNode(Job job, TimeSpan? startTime = null, bool ignoreFirstStopExecutionTime = false);

        double[,] Array { get; }

        double[,] SetTravelTimesResults(IList<RouteStop> routeStops);
        double[,] SetTimeWindowValuesResults(IList<RouteStop> routeStops);
        double[,] SetServiceTimesResults(IList<RouteStop> routeStops, bool ignoreFirstStopExecutionTime);
        double[,] SetSumValuesResults(IList<RouteStop> routeStops);
        double[,] SetResultTimeWindowsResults(IList<RouteStop> routeStops);
        double[,] SetDepartureTimeResults(TimeSpan? startTime, IList<RouteStop> routeStops);
        double[,] SetViolationsAndWaitResults(IList<RouteStop> routeStops);
        int GetMatrixIndex(string matrixRowName);
    }

    public class JobNodeService : IJobNodeService
    {
        private readonly IDistanceService _distanceService;

        private double[,] _array;
        public double[,] Array { get { return _array; } }
        const int RowIndexWindowLowerBounds = 0;
        const int RowIndexWindowUpperBounds = 1;
        const int RowIndexTravelTime = 2;
        const int RowIndexArrival = 3;
        const int RowIndexServiceTime = 4;
        const int RowIndexWaitTime = 5;
        const int RowIndexDepartureTime = 6;
        const int RowIndexViolations = 7;
        const int RowIndexSummation = 8;
        const int RowIndexResultTwl = 9;
        const int RowIndexResultTwu = 10;

        public JobNodeService(IDistanceService distanceService)
        {
            _distanceService = distanceService;
        }
        
        public int GetMatrixIndex(string matrixRowName)
        {
            var results = -1;
            if (!matrixRowName.StartsWith("RowIndex"))
            {
                matrixRowName = "RowIndex" + matrixRowName;
            }
            var temp = typeof(JobNodeService).GetField(matrixRowName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (temp != null)
            {
                results = (int)temp.GetRawConstantValue();    
            }
            return results;
        }

        // CBS 10 September 2014
        // Remove LU stops at FEC MIAMI TERMINAL when preceeded by PLWC stop at FEC MIAMI RAIL
        protected IEnumerable<RouteStop> FilterExtraniousFecMiamiTerminalStops(IList<RouteStop> routeStops)
        {
            if (routeStops.Count() > 1)
            {
                for (var i = 1; i < routeStops.Count; i++)
                {
                    var j = i - 1;
                    if (routeStops[j].Location.DisplayName == "FEC MIAMI RAIL" && routeStops[j].StopAction.ShortName == "PLWC")
                    {
                        if (routeStops[i].Location.DisplayName == "FEC MIAMI TERMINAL" && routeStops[i].StopAction.ShortName == "LU")
                        {
                            routeStops.Remove(routeStops[i]);
                        }
                    }
                }
            }
            return routeStops;
        } 

        private void SetDepartureTimeFromStartTime(IList<RouteStop> routeStops, TimeSpan? startTime)
        {
            if (startTime.HasValue)
            {
                for (var i = 0; i < routeStops.Count; i++)
                {
                    if (i == 0)
                    {
                        // set departure time
                        _array[RowIndexDepartureTime, i] = startTime.Value.TotalSeconds;
                    }
                    else
                    {
                        // set departure time
                        var prevDepartureTime = _array[RowIndexDepartureTime, i - 1];
                        var stopTime = _array[RowIndexServiceTime, i];
                        var departureTime = prevDepartureTime + stopTime;

                        _array[RowIndexDepartureTime, i] = departureTime;
                    }
                }
            }
            else
            {
                // blank values
                for (var i = 0; i < routeStops.Count; i++)
                {
                    _array[RowIndexDepartureTime, i] = 0;
                }
            }
        }

        private void SetWaitTime(IList<RouteStop> routeStops, TimeSpan? startTime)
        {
            if (startTime.HasValue)
            {
                for (var i = 0; i < routeStops.Count; i++)
                {
                    if (i == 0)
                    {
                        _array[RowIndexWaitTime, i] = 0;
                    }
                    else
                    {
                        var arrivalTime = _array[RowIndexArrival, i];
                        var twl = _array[RowIndexWindowLowerBounds, i];
                        var waitTime = arrivalTime > twl ? arrivalTime - twl : 0;
                        
                        _array[RowIndexWaitTime, i] = waitTime;
                    }
                }
            }
            else
            {
                // blank values
                for (var i = 0; i < routeStops.Count; i++)
                {
                    _array[RowIndexWaitTime, i] = 0;
                }
            }
        }

        public JobNode CreateJobNode(Job job, TimeSpan? startTime = null, bool ignoreFirstStopExecutionTime = false)
        {
            var routeStops = FilterExtraniousFecMiamiTerminalStops(job.RouteStops.ToList()).ToList();
            
            job.RouteStops = routeStops;

            job.RouteSegmentStatisticses = new List<RouteSegmentStatistics>();

            _array = new double[11, routeStops.Count];

            if (job.RouteStops != null && job.RouteStops.Any())
            {
                SetTravelTimes(routeStops);
                SetTimeWindowValues(routeStops);
                SetServiceTimes(routeStops, startTime == null || ignoreFirstStopExecutionTime);
                SetSumValues(routeStops);
                SetResultTimeWindows(routeStops);

                SetDepartureTime(startTime, routeStops);
                //SetArrivalTime(routeStops);
                SetViolationsAndWait(routeStops);
                
                var result = new JobNode(job, _array)
                    {
                        Priority = job.Priority > 0 && job.Priority < 4 ? job.Priority : 1
                    };

                // TODO URGENT - use automapper to assure that future 
                //  properties will be mapped automatically without needing to create a manual mapping

                try { result.Job.IsHazmat = job.IsHazmat; }
                catch (Exception e) { Console.WriteLine(e.ToString()); }

                try { result.WindowStart = TimeSpan.FromSeconds(_array[RowIndexResultTwl, 0]); }
                catch { result.WindowStart = TimeSpan.Zero; }

                try { result.WindowEnd = TimeSpan.FromSeconds(_array[RowIndexResultTwu, 0]); }
                catch { result.WindowEnd = _array[RowIndexResultTwu, 0] > 0 ? TimeSpan.MaxValue : result.WindowStart; }

                
                if (result.WindowEnd < result.WindowStart)
                {
                    result.WindowStart = result.WindowEnd;
                }

                if (DoesViolationExist(routeStops))
                {
                    result.IsInvalid = true;
                    //throw new Exception("Invalid Jobnode - violation detected");
                }

                for (var i = 1; i < routeStops.Count; i++)
                {
                    var distance = _distanceService.CalculateDistance(routeStops[i - 1].Location, routeStops[i].Location);
                    var rss = new RouteSegmentStatistics()
                        {
                            StartStop = routeStops[i - 1],
                            EndStop = routeStops[i],
                            StartTime = TimeSpan.FromSeconds(_array[RowIndexArrival, i]),
                            WhiffedTimeWindow = _array[RowIndexViolations, i] == 0,
                            Statistics = new RouteStatistics
                                {
                                    TotalExecutionTime = TimeSpan.FromSeconds(_array[RowIndexServiceTime, i]),
                                    TotalIdleTime = TimeSpan.FromSeconds(_array[RowIndexWaitTime, i]),
                                    TotalQueueTime = TimeSpan.Zero,
                                    TotalTravelTime = distance.Time,
                                    TotalTravelDistance = distance.Distance
                                }
                        };
                    job.RouteSegmentStatisticses.Add(rss);
                }

                return result;
            }

            return new JobNode(job) { IsInvalid=true};
        }

        public double[,] SetTravelTimesResults(IList<RouteStop> routeStops)
        {
            SetTravelTimes(routeStops);
            return _array;
        }

        public double[,] SetTimeWindowValuesResults(IList<RouteStop> routeStops)
        {
            SetTimeWindowValues(routeStops);
            return _array;
        }

        public double[,] SetServiceTimesResults(IList<RouteStop> routeStops, bool ignoreFirstStopExecutionTime)
        {
            SetServiceTimes(routeStops, ignoreFirstStopExecutionTime);
            return _array;
        }

        public double[,] SetSumValuesResults(IList<RouteStop> routeStops)
        {
            SetSumValues(routeStops);
            return _array;
        }

        public double[,] SetResultTimeWindowsResults(IList<RouteStop> routeStops)
        {
            SetResultTimeWindows(routeStops);
            return _array;
        }

        public double[,] SetDepartureTimeResults(TimeSpan? startTime, IList<RouteStop> routeStops)
        {
            SetDepartureTime(startTime, routeStops);
            return _array;
        }

        public double[,] SetViolationsAndWaitResults(IList<RouteStop> routeStops)
        {
            SetViolationsAndWait(routeStops);
            return _array;
        }

        private void SetTravelTimes(IList<RouteStop> routeStops)
        {
            for (var i = 1; i < routeStops.Count; i++)
            {
                var rsOrigin = routeStops[i - 1];
                var rsDestination = routeStops[i];
                var distance = _distanceService.CalculateDistance(rsOrigin.Location, rsDestination.Location);
                _array[RowIndexTravelTime, i] = distance.Time.TotalSeconds;
            }
        }

        private void SetServiceTimes(IList<RouteStop> routeStops, bool ignoreFirstStopExecutionTime)
        {
            for (var i = 0; i < routeStops.Count; i++)
            {
                var rs = routeStops[i];
                var rsExecutionTime = rs.ExecutionTime.HasValue ? rs.ExecutionTime.Value.TotalSeconds : 0;
                if (i == 0 && ignoreFirstStopExecutionTime)
                {
                    rsExecutionTime = 0;
                }
                _array[RowIndexServiceTime, i] = rsExecutionTime;
            }
        }


        private void SetTimeWindowValues(IList<RouteStop> routeStops)
        {
            for (var i = 0; i < routeStops.Count; i++)
            {
                var rs = routeStops[i];
                _array[RowIndexWindowLowerBounds, i] = (double)rs.WindowStart.TotalSeconds;
                _array[RowIndexWindowUpperBounds, i] = (double)rs.WindowEnd.TotalSeconds;
            }
        }

        private void SetSumValues(IList<RouteStop> routeStops)
        {
            for (var i = 0; i < routeStops.Count; i++)
            {
                if (i == 0)
                {
                    _array[RowIndexSummation, i] = 0;
                    continue;
                }

                var previousSum = _array[RowIndexSummation, i - 1];
                var previousStopTime = _array[RowIndexServiceTime, i - 1];
                var currentTravelTime = _array[RowIndexTravelTime, i];

                _array[RowIndexSummation, i] = previousSum + previousStopTime + currentTravelTime;
            }
        }

        private bool DoesViolationExist(IList<RouteStop> routeStops)
        {
            var result = false;
            for (var i = 0; i < routeStops.Count; i++)
            {
                if (_array[RowIndexViolations, i] > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        private void SetResultTimeWindows(IList<RouteStop> routeStops)
        {
            for (var i = 0; i < routeStops.Count; i++)
            {
                if (i > 0)
                {
                    var twl = Convert.ToDouble(_array[RowIndexWindowLowerBounds, i] - _array[RowIndexSummation, i]);
                    var twu = Convert.ToDouble(_array[RowIndexWindowUpperBounds, i] - _array[RowIndexSummation, i]);

                    _array[RowIndexResultTwl, i] = twl > 0 ? twl : 0;
                    _array[RowIndexResultTwu, i] = twu > 0 ? twu : 0;
                }
            }

            double? highestTwl = null;
            double? lowestTwu = null;

            for (var i = 1; i < routeStops.Count; i++)
            {
                if (i > 0)
                {
                    var twl = _array[RowIndexResultTwl, i];
                    var twu = _array[RowIndexResultTwu, i];

                    if (!highestTwl.HasValue || twl > highestTwl.Value)
                    {
                        highestTwl = twl;
                    }

                    if (!lowestTwu.HasValue || twu < lowestTwu.Value)
                    {
                        lowestTwu = twu;
                    }
                }

                var resultTwl = highestTwl.HasValue ? highestTwl.Value : 0;
                var resultTwu = lowestTwu.HasValue ? lowestTwu.Value : 0;

                _array[RowIndexResultTwl, 0] = resultTwl;
                _array[RowIndexResultTwu, 0] = resultTwu;
            }
        }

        private void SetDepartureTime(TimeSpan? startTime, IList<RouteStop> routeStops)
        {
            var i = 0;
            // for the first stop
            var arrivalTime = Math.Min(_array[RowIndexResultTwl, 0], _array[RowIndexResultTwu, 0]);
            _array[RowIndexArrival, i] = Math.Floor(arrivalTime);
            _array[RowIndexDepartureTime, i] = _array[RowIndexArrival, i] + _array[RowIndexServiceTime, i];
            // subsequent stops
            for (i = 1; i < routeStops.Count; i++)
            {
                _array[RowIndexArrival, i] = _array[RowIndexDepartureTime, i - 1] + _array[RowIndexTravelTime, i];
                UpdateDepartureTime(routeStops[i], i);
            }
        }

        private void UpdateDepartureTime(RouteStop routeStop, int i)
        {
            var arrivalTime = _array[RowIndexArrival, i];
            var serviceTime = _array[RowIndexServiceTime, i];
            var twl = _array[RowIndexWindowLowerBounds, i];

            if (arrivalTime >= twl)
            {
                _array[RowIndexDepartureTime, i] = arrivalTime + serviceTime;
            }
            else if (arrivalTime < twl)
            {
                _array[RowIndexDepartureTime, i] = twl + serviceTime;
            }
        }

        private void SetArrivalTime(IList<RouteStop> routeStops)
        {
            for (var i = 0; i < routeStops.Count; i++)
            {

                if (i == 0)
                {
                    var arrivalMin = Math.Min(_array[RowIndexResultTwl, 0], _array[RowIndexResultTwu, 0]);
                    _array[RowIndexArrival, i] = arrivalMin;
                }

                else
                {
                    var previousDepartureTime = _array[RowIndexDepartureTime, i - 1];
                    var travelTime = _array[RowIndexTravelTime, i];

                    var arrivalTime = previousDepartureTime + travelTime;
                    _array[RowIndexArrival, i] = arrivalTime;
                }
            }
        }

        private void SetViolationsAndWait(IList<RouteStop> routeStops)
        {
            for (var i = 0; i < routeStops.Count; i++)
            {
                var arrivalTime = _array[RowIndexArrival, i];
                var twl = _array[RowIndexWindowLowerBounds, i];
                var twu = _array[RowIndexWindowUpperBounds, i];
                var isWait = arrivalTime < twl;
                var isViolation = arrivalTime > twu;

                if (isViolation)
                {
                    _array[RowIndexViolations, i] = 1;
                }
                else
                {
                    if (isWait)
                    {
                        if (twl > arrivalTime)
                        {
                            _array[RowIndexWaitTime, i] = twl - arrivalTime;
                        }
                        else
                        {
                            _array[RowIndexWaitTime, i] = 0;
                        }
                    }
                    else
                {
                        _array[RowIndexWaitTime, i] = 0;
                    }
                }



            }
        }

    }
}