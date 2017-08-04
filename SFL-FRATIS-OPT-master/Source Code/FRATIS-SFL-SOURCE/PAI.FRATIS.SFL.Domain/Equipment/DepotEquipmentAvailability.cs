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

namespace PAI.FRATIS.SFL.Domain.Equipment
{
    public class DepotEquipmentAvailability : EntitySubscriberBase
    {
        /// <summary>
        /// Gets or sets the Depot
        /// </summary>
        public virtual Depot Depot { get; set; }
        public virtual int? DepotId { get; set; }

        /// <summary>
        /// Gets or sets the container owner
        /// </summary>
        public virtual ContainerOwner ContainerOwner { get; set; }
        public virtual int? ContainerOwnerId { get; set; }

        /// <summary>
        /// Gets or sets the container type
        /// </summary>
        public virtual Container Container { get; set; }
        public virtual int? ContainerId { get; set; }

        /// <summary>
        /// Gets or sets the chassis owner
        /// </summary>
        public virtual ChassisOwner ChassisOwner { get; set; }
        public virtual int? ChassisOwnerId { get; set; }

        /// <summary>
        /// Gets or sets the ChassisType
        /// </summary>
        public virtual Chassis Chassis { get; set; }
        public virtual int? ChassisId { get; set; }

    }
}
