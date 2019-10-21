using System;
using System.Linq;
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Orders;

namespace Tests
{
    public interface IJobHelper
    {
        RouteStop CreateRouteStop(Job job, int minutes, StopAction sa, Location location,
                                          TimeSpan stopDelay, TimeSpan windowStart, TimeSpan windowEnd);

        TimeSpan GetTimeSpan(int minutes);
    }

    public class JobHelper : IJobHelper
    {
        public RouteStop CreateRouteStop(Job job, int minutes, StopAction sa, Location location, TimeSpan stopDelay, TimeSpan windowStart, TimeSpan windowEnd)
        {
            if (sa == null) { throw new Exception("Stop Action is required"); }
            if (job == null) { throw new Exception("Job is invalid"); }

            return new RouteStop()
                {
                    Id = 1,
                    Location = location,
                    PostTruckConfig = null,
                    PreTruckConfig = null,
                    StopAction = sa,
                    StopDelay = stopDelay,
                    WindowStart = windowStart,
                    WindowEnd = windowEnd
                };
        }

        public TimeSpan GetTimeSpan(int minutes)
        {
            return new TimeSpan(0, minutes, 0);
        }

        public TimeSpan GetTimeSpan(int hours, int minutes)
        {
            return new TimeSpan(hours, minutes, 0);
        }

    }
}
