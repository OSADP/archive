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
using System.Linq;
using PAI.Drayage.Optimization.Model.Equipment;

namespace PAI.Drayage.Optimization.Model.Orders
{
    /// <summary>
    /// Represents a route stop
    /// </summary>
    public partial class RouteStop : ModelBase
    {
        /// <summary>
        /// Gets or sets stop action
        /// </summary>
        public virtual StopAction StopAction { get; set; }
        
        /// <summary>
        /// Gets or sets the location
        /// </summary>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the start of the date range
        /// </summary>
        public virtual TimeSpan WindowStart { get; set; }

        /// <summary>
        /// Gets or sets the end of the date range
        /// </summary>
        public virtual TimeSpan WindowEnd { get; set; }
        
        /// <summary>
        /// Gets or sets the truck configuration that must be met at this route stop
        /// </summary>
        public virtual TruckConfiguration PreTruckConfig { get; set; }

        /// <summary>
        /// Gets or sets the truck configuration after the route stop
        /// </summary>
        public virtual TruckConfiguration PostTruckConfig { get; set; }

        /// <summary>
        /// Gets or sets the ExecutionTime (optional)
        /// </summary>
        public virtual TimeSpan? ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the QueueTime (optional)
        /// </summary>
        public virtual TimeSpan? QueueTime { get; set; }
        
        public RouteStop()
        {
            WindowStart = TimeSpan.Zero;
            WindowEnd = TimeSpan.MaxValue;
            StopAction = StopActions.NoAction;
            PreTruckConfig = new TruckConfiguration();
            PostTruckConfig = new TruckConfiguration();
        }
    }
}