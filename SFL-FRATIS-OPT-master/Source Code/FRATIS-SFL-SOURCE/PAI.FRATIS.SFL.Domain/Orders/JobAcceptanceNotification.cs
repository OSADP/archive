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

namespace PAI.FRATIS.SFL.Domain.Orders
{
    /// <summary>
    /// A notification message to be sent upon acceptance of Job/Routestop by Driver
    /// Used for Dual Transactions at same location notification
    /// </summary>
    public partial class JobAcceptanceNotification : EntitySubscriberBase
    {
        public List<int> JobIds { get; set; }

        public List<int> RouteStopIds { get; set; }

        public string MessageBody { get; set; }
    }
}
