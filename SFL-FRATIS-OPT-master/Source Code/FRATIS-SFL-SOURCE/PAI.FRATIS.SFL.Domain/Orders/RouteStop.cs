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
using PAI.FRATIS.SFL.Domain.Geography;

namespace PAI.FRATIS.SFL.Domain.Orders
{
    /// <summary>
    /// Represents a route stop
    /// </summary>
    public partial class RouteStop : EntitySubscriberBase, IDatedEntity, ISortableEntity
    {
        /// <summary>
        /// Gets or sets the associated order 
        /// </summary>
        public virtual Job Job { get; set; }
        public virtual int? JobId { get; set; }

        /// <summary>
        /// Gets or sets the sort order
        /// </summary>
        public virtual int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets whether this is a dynamic stop
        /// </summary>
        public bool IsDynamicStop { get; set; }

        /// <summary>
        /// Gets or sets stop action
        /// </summary>
        public virtual StopAction StopAction { get; set; }
        public int? StopActionId { get; set; }
        
        /// <summary>
        /// Gets or sets the location
        /// </summary>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the location id
        /// </summary>
        public int? LocationId { get; set; }

        public long WindowStart { get; set; }

        public long WindowEnd { get; set; }

        /// <summary>
        /// Gets or sets the stop delay in ticks
        /// </summary>
        public virtual long? StopDelay { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? EstimatedETA { get; set; }

        public DateTime? ActualETA { get; set; }

    }
}