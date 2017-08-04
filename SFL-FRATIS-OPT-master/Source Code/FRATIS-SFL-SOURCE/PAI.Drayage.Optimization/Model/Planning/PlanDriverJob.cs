using System;
using System.Collections.Generic;
using System.Linq;

using PAI.Drayage.Domain.Planning;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Planning
{
    /// <summary>
    /// represents a driver plan
    /// </summary>
    public partial class PlanDriverJob : ModelBase
    {
        /// <summary>
        /// Gets or sets the JobPlans
        /// </summary>
        public Job Job { get; set; }

        /// <summary>
        /// Gets or sets the Sort Order
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the route stops
        /// </summary>
        private ICollection<RouteStop> _routeStops = null;
        public ICollection<RouteStop> RouteStops
        {
            get
            {
                return _routeStops ?? (_routeStops = new List<RouteStop>());
            }
            set
            {
                _routeStops = value;
            }
        }

        public long DepartureTime { get; set; }
        public TimeSpan DepartureTimeSpan 
        {
            get { return new TimeSpan(DepartureTime); }
        }

        public virtual ICollection<RouteSegmentMetric> Metrics { get; set; }

    }
}