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

namespace PAI.Drayage.Optimization.Model.Equipment
{
    /// <summary>
    /// A container for chassis, container, and the owner of each
    /// </summary>
    public interface IEquipmentConfiguration
    {
        Chassis Chassis { get; set; }
        ChassisOwner ChassisOwner { get; set; }
        Container Container { get; set; }
        ContainerOwner ContainerOwner { get; set; }
    }

    /// <summary>
    /// A container for chassis, container, and the owner of each
    /// </summary>
    public class EquipmentConfiguration : IEquipmentConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentConfiguration"/> class.
        /// </summary>
        public EquipmentConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentConfiguration"/> class.
        /// </summary>
        /// <param name="chassisType">Type of the chassis.</param>
        /// <param name="containerType">Type of the container.</param>
        /// <param name="chassisOwner">The chassis owner.</param>
        /// <param name="containerOwner">The container owner.</param>
        public EquipmentConfiguration(Chassis chassisType, Container containerType, ChassisOwner chassisOwner, ContainerOwner containerOwner)
        {
            Chassis = chassisType;
            Container = containerType;
            ChassisOwner = chassisOwner;
            ContainerOwner = containerOwner;
        }

        /// <summary>
        /// Gets or sets the chassis.
        /// </summary>
        /// <value>
        /// The chassis.
        /// </value>
        public Chassis Chassis { get; set; }

        /// <summary>
        /// Gets or sets the chassis owner.
        /// </summary>
        /// <value>
        /// The chassis owner.
        /// </value>
        public ChassisOwner ChassisOwner { get; set; }

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <value>
        /// The container.
        /// </value>
        public Container Container { get; set; }

        /// <summary>
        /// Gets or sets the container owner.
        /// </summary>
        /// <value>
        /// The container owner.
        /// </value>
        public ContainerOwner ContainerOwner { get; set; }
    }
}