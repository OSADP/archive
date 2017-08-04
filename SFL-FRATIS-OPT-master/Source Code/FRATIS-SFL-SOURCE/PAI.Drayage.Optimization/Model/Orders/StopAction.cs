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
    /// Represents a stop action or type
    /// </summary>
    public partial class StopAction : ModelBase
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the short name
        /// </summary>
        public virtual string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the state prior to executing action
        /// </summary>
        public virtual TruckState PreState { get; set; }
        
        /// <summary>
        /// Gets or sets the state upon executing action
        /// </summary>
        public virtual TruckState PostState { get; set; }

        /// <summary>
        /// Gets or sets the action
        /// </summary>
        public Action Action { get; set; }
    }
}