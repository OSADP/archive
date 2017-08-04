//    Copyright 2013 Productivity Apex Inc.
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

namespace PAI.CTIP.Optimization.Model.Equipment
{
    public class TruckConfiguration
    {
        public bool IsLoaded { get; set; }

        public TruckState TruckState
        {
            get { return GetTruckState(); }
        }

        public EquipmentConfiguration EquipmentConfiguration { get; set; }

        public TruckConfiguration()
        {
            EquipmentConfiguration = new EquipmentConfiguration();
        }

        public TruckConfiguration(Chassis chassisType, Container containerType, ChassisOwner chassisOwner, ContainerOwner containerOwner, bool isLoaded)
        {
            EquipmentConfiguration = new EquipmentConfiguration(chassisType, containerType, chassisOwner, containerOwner);
            IsLoaded = isLoaded;
        }

        public bool IsMatchChassis(TruckConfiguration config)
        {
            if (EquipmentConfiguration.Chassis == null || config.EquipmentConfiguration.Chassis == null)
            {
                return false;
            }

            return (EquipmentConfiguration.Chassis == config.EquipmentConfiguration.Chassis && 
                EquipmentConfiguration.ChassisOwner == config.EquipmentConfiguration.ChassisOwner);
        }

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

        public static bool operator ==(TruckConfiguration c1, TruckConfiguration c2)
        {
            return c1.EquipmentConfiguration.Chassis == c2.EquipmentConfiguration.Chassis &&
                c1.EquipmentConfiguration.Container == c2.EquipmentConfiguration.Container &&
                c1.EquipmentConfiguration.ContainerOwner == c2.EquipmentConfiguration.ContainerOwner &&
                c1.IsLoaded == c2.IsLoaded;
        }
        
        public static bool operator !=(TruckConfiguration c1, TruckConfiguration c2)
        {
            return c1.EquipmentConfiguration.Chassis != c2.EquipmentConfiguration.Chassis ||
                c1.EquipmentConfiguration.Container != c2.EquipmentConfiguration.Container ||
                c1.EquipmentConfiguration.ContainerOwner != c2.EquipmentConfiguration.ContainerOwner ||
                c1.IsLoaded != c2.IsLoaded;
        }
        
    }
}
