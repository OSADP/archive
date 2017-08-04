using System;
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Planning
{
    /// <summary>
    /// represents a driver plan
    /// </summary>
    public partial class PlanDriver : ModelBase
    {
        /// <summary>
        /// Gets or sets the Driver
        /// </summary>
        public Driver Driver { get; set; }

        /// <summary>
        /// Gets or sets the JobPlans
        /// </summary>
        public IList<PlanDriverJob> JobPlans { get; set; }
        
        /// <summary>
        /// Gets or sets the Route Segment Statistics
        /// </summary>
        public IList<RouteSegmentStatistics> RouteSegmentStatistics { get; set; }

        public long DepartureTime { get; set; }

        public TimeSpan DepartureTimeSpan
        {
            get
            {
                return new TimeSpan(DepartureTime);
            }
            set
            {
                DepartureTime = value.Ticks;
            }
        }

    }
}
