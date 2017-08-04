﻿//    Copyright 2014 Productivity Apex Inc.
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
using PAI.FRATIS.SFL.Domain.Orders;

namespace PAI.FRATIS.SFL.Domain.Planning
{
    public partial class PlanDriverJob : EntitySubscriberBase, ISortableEntity
    {
        public virtual PlanDriver PlanDriver { get; set; }

        /// <summary>
        /// Gets or sets the Driver
        /// </summary>
        public virtual Job Job { get; set; }
        public virtual int JobId { get; set; }

        /// <summary>
        /// Gets or sets the sort order
        /// </summary>
        public virtual int SortOrder { get; set; }

        public long DepartureTime { get; set; }

        public TimeSpan DepartureTimeSpan
        {
            get
            {
                return new TimeSpan(DepartureTime);
            }
        }

        public ICollection<RouteSegmentMetric> Metrics { get; set; }

        ///// <summary>
        ///// Gets or sets the route stops
        ///// </summary>
        //private ICollection<RouteStop> _routeStops = null;
        //public virtual ICollection<RouteStop> RouteStops
        //{
        //    get
        //    {
        //        return _routeStops ?? (_routeStops = new List<RouteStop>());
        //    }
        //    set
        //    {
        //        _routeStops = value;
        //    }
        //}
    }
}