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
    /// The state of the chassis and container, the state of the load, and validation information for a vehicle
    /// </summary>
    public class TruckConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Gets the state of the truck.
        /// </summary>
        /// <value>
        /// The state of the truck.
        /// </value>
        public TruckState TruckState
        {
            get { return GetTruckState(); }
        }

        /// <summary>
        /// Gets or sets the equipment configuration.
        /// </summary>
        /// <value>
        /// The equipment configuration.
        /// </value>
        public EquipmentConfiguration EquipmentConfiguration { get; set; }

        public TruckConfiguration()
        {
            EquipmentConfiguration = new EquipmentConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruckConfiguration"/> class.
        /// </summary>
        /// <param name="chassisType">Type of the chassis.</param>
        /// <param name="containerType">Type of the container.</param>
        /// <param name="chassisOwner">The chassis owner.</param>
        /// <param name="containerOwner">The container owner.</param>
        /// <param name="isLoaded">if set to <c>true</c> [is loaded].</param>
        public TruckConfiguration(Chassis chassisType, Container containerType, ChassisOwner chassisOwner, ContainerOwner containerOwner, bool isLoaded)
        {
            EquipmentConfiguration = new EquipmentConfiguration(chassisType, containerType, chassisOwner, containerOwner);
            IsLoaded = isLoaded;
        }

        /// <summary>
        /// Determines whether the chassis is appropriate for the supplied configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>true if valid, otherwise false</returns>
        public bool IsMatchChassis(TruckConfiguration config)
        {
            if (EquipmentConfiguration.Chassis == null || config.EquipmentConfiguration.Chassis == null)
            {
                return false;
            }

            return (EquipmentConfiguration.Chassis == config.EquipmentConfiguration.Chassis && 
                EquipmentConfiguration.ChassisOwner == config.EquipmentConfiguration.ChassisOwner);
        }

        /// <summary>
        /// Determines whether the container is appropriate for the sopplied configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>true if valid, otherwise false</returns>
        public bool IsMatchContainer(TruckConfiguration config)
        {
            return (EquipmentConfiguration.Container != null && 
                EquipmentConfiguration.ContainerOwner != null &&
                config.EquipmentConfiguration.Container != null &&
                config.EquipmentConfiguration.ContainerOwner != null &&
                EquipmentConfiguration.Container == config.EquipmentConfiguration.Container && 
                EquipmentConfiguration.ContainerOwner == config.EquipmentConfiguration.ContainerOwner);
        }

        private TruckState GetTruckState()
        {
            if (EquipmentConfiguration.Chassis == null && EquipmentConfiguration.Container == null)
                return TruckState.Bobtail;

            if (EquipmentConfiguration.Chassis != null && EquipmentConfiguration.Container == null)
                return TruckState.Chassis;

            if (EquipmentConfiguration.Chassis != null && EquipmentConfiguration.Container != null && !IsLoaded)
                return TruckState.Empty;

            if (EquipmentConfiguration.Chassis != null && EquipmentConfiguration.Container != null && IsLoaded)
                return TruckState.Loaded;

            return TruckState.Invalid;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="c1">The left hand operand.</param>
        /// <param name="c2">The right hand operand.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(TruckConfiguration c1, TruckConfiguration c2)
        {
            return c1.EquipmentConfiguration.Chassis == c2.EquipmentConfiguration.Chassis &&
                c1.EquipmentConfiguration.Container == c2.EquipmentConfiguration.Container &&
                c1.EquipmentConfiguration.ContainerOwner == c2.EquipmentConfiguration.ContainerOwner &&
                c1.IsLoaded == c2.IsLoaded;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="c1">The left hand operand.</param>
        /// <param name="c2">The right hand operand.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(TruckConfiguration c1, TruckConfiguration c2)
        {
            return c1.EquipmentConfiguration.Chassis != c2.EquipmentConfiguration.Chassis ||
                c1.EquipmentConfiguration.Container != c2.EquipmentConfiguration.Container ||
                c1.EquipmentConfiguration.ContainerOwner != c2.EquipmentConfiguration.ContainerOwner ||
                c1.IsLoaded != c2.IsLoaded;
        }
        
    }
}
