using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Services
{
    public interface INodeFactory
    {
        JobNode CreateJobNode(Job job);
    }

    public class NodeFactory : INodeFactory
    {
        private readonly IRouteStopService _routeStopService;

        public NodeFactory(IRouteStopService routeStopService)
        {
            _routeStopService = routeStopService;
        }

        /// <summary>
        /// Creates a JobNode from the provided Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public JobNode CreateJobNode(Job job)
        {
            var result = new JobNode()
            {
                Job = job,
                RouteStops = job.RouteStops.ToList(),
                WindowStart = job.RouteStops.ToList().First().WindowStart,
                WindowEnd = job.RouteStops.ToList().First().WindowEnd,
                Priority = job.Priority > 0 && job.Priority < 4 ? job.Priority : 1,
            };

            var indexOfFirstWindow = GetIndexOfFirstRouteStopWithWindow(result);
            for (int i = indexOfFirstWindow; i > 0; i--)
            {
                var rs = result.RouteStops[i];
                var nextStop = result.RouteStops[i - 1];

                var routeSegmentStatistics = _routeStopService.CreateRouteSegmentStatistics(rs.WindowStart, rs, nextStop);
                var delay = routeSegmentStatistics.Statistics.TotalTravelTime + rs.StopDelay;
                
                result.WindowEnd = rs.WindowEnd - delay.Value;
            }

            return result;
        }

        private int GetIndexOfFirstRouteStopWithWindow(JobNode jobNode)
        {
            for (int i = 0; i < jobNode.RouteStops.Count; i++)
            {
                var rs = jobNode.RouteStops[i];
                var duration = rs.WindowEnd - rs.WindowStart;

                if (duration < new TimeSpan(23, 59, 0))
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfLastRouteStopWithWindow(JobNode jobNode)
        {
            for (int i = jobNode.RouteStops.Count; i > 0; i--)
            {
                var rs = jobNode.RouteStops[i - 1];
                var duration = rs.WindowEnd - rs.WindowStart;

                if (duration < new TimeSpan(23, 59, 0))
                {
                    return i - 1;
                }
            }

            return -1;
        }
    }
}
