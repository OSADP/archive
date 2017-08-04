//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using PAI.FRATIS.SFL.Domain.Orders;

namespace PAI.FRATIS.SFL.Domain.Planning
{
    /// <summary>
    /// represents a driver plan
    /// </summary>
    public partial class PlanDriver : EntitySubscriberBase
    {
        /// <summary>
        /// Gets or sets the Driver
        /// </summary>
        public virtual Driver Driver { get; set; }
        public virtual int DriverId { get; set; }

        private ICollection<PlanDriverJob> _jobPlans = null;

        /// <summary>
        /// Gets or sets the jobs
        /// </summary>
        public virtual ICollection<PlanDriverJob> JobPlans
        {
            get { return _jobPlans ?? (_jobPlans = new List<PlanDriverJob>()); }
            set { _jobPlans = value; }
        }

        /// <summary>
        /// Gets or sets the route segment metrics
        /// </summary>
        private IList<RouteSegmentMetric> _routeSegmentMetrics = null;
        public virtual IList<RouteSegmentMetric> RouteSegmentMetrics
        {
            get
            {
                return _routeSegmentMetrics ?? (_routeSegmentMetrics = new List<RouteSegmentMetric>());
            }
            set
            {
                _routeSegmentMetrics = value;
            }
        }

        public long DepartureTime { get; set; }

        public TimeSpan DepartureTimeSpan
        {
            get
            {
                return new TimeSpan(DepartureTime);
            }
        }

        public RouteSegmentMetric TotalMetrics
        {
            get
            {
                var result = new RouteSegmentMetric();
                if (RouteSegmentMetrics != null)
                {
                    result = RouteSegmentMetrics.Aggregate(result, (current, metric) => current + metric);
                }
                return result;
            }
        }
    }
}