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
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Node
{
    /// <summary>
    /// A driver with the route scheduled for them
    /// </summary>
    public class DriverNode : NodeBase
    {
        /// <summary>
        /// Gets the driver.
        /// </summary>
        /// <value>
        /// The driver.
        /// </value>
        public Driver Driver { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverNode"/> class.
        /// </summary>
        /// <param name="driver">The driver.</param>
        public DriverNode(Driver driver)
        {
            Id = driver.Id;
            Driver = driver;
            RouteStops = new List<RouteStop>
                {
                    new RouteStop() { Location = driver.StartingLocation}
                };
            WindowStart = new TimeSpan(driver.EarliestStartTime);
            WindowEnd = new TimeSpan(driver.EarliestStartTime).Add(TimeSpan.FromHours(18));
            Priority = 1.0;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Driver " + Driver.DisplayName;
        }

    }
}