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

namespace PAI.FRATIS.SFL.Domain.Orders
{
    /// <summary>
    /// Represents a drayage job that has been accepted and navigated to
    /// by a driver, as reported by the TomTom WEBFLEET Background Process
    /// </summary>
    public partial class JobAcceptance : EntitySubscriberBase
    {
        public int? JobId { get; set; }
        public virtual Job Job { get; set; }

        public DateTime? AcceptedTime { get; set; }
        public DateTime? ETATime { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public int? DriverId { get; set; }

    }
}
