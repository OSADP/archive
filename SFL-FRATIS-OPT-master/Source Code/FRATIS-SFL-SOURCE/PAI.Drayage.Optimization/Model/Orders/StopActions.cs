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
using PAI.Drayage.Optimization.Model.Equipment;

namespace PAI.Drayage.Optimization.Model.Orders
{
    /// <summary>
    /// Contains the types of actions used at a stop
    /// </summary>
    public class StopActions
    {
        private static List<StopAction> _stopActions;
        public static IEnumerable<StopAction> Actions
        {
            get
            {
                if (_stopActions == null)
                {
                    _stopActions = new List<StopAction>()
                        {
                            NoAction,
                            PickupChassis,
                            DropOffChassis,
                            PickupEmpty,
                            DropOffEmpty,
                            PickupEmptyWithChassis,
                            DropOffEmptyWithChassis,
                            PickupLoaded,
                            DropOffLoaded,
                            PickupLoadedWithChassis,
                            DropOffLoadedWithChassis,
                            LiveLoading,
                            LiveUnloading
                        };
                }

                return _stopActions;
            } 
        }
        
        public static StopAction NoAction = new StopAction()
            {
                Id = 1,
                Name = "No Action",
                ShortName = "NA",
                Action = Action.NoAction,
                PreState = TruckState.Bobtail,
                PostState = TruckState.Any,
            };

        public static StopAction PickupChassis = new StopAction()
            {
                Id = 2,
                Name = "Pickup Chassis",
                ShortName = "PC",
                Action = Action.PickUp,
                PreState = TruckState.Bobtail,
                PostState = TruckState.Chassis,
            };

        public static StopAction DropOffChassis = new StopAction()
            {
                Id = 3,
                Name = "Drop Off Chassis",
                ShortName = "DC",
                Action = Action.DropOff,
                PreState = TruckState.Chassis,
                PostState = TruckState.Bobtail,
            };

        public static StopAction PickupEmpty = new StopAction()
            {
                Id = 4,
                Name = "Pickup Empty",
                ShortName = "PE",
                Action = Action.PickUp,
                PreState = TruckState.Chassis,
                PostState = TruckState.Empty,
            };

        public static StopAction DropOffEmpty = new StopAction()
            {
                Id = 5,
                Name = "Drop Off Empty",
                ShortName = "DE",
                Action = Action.DropOff,
                PreState = TruckState.Empty,
                PostState = TruckState.Chassis,
            };

        public static StopAction PickupLoaded = new StopAction()
            {
                Id = 6,
                Name = "Pickup Loaded",
                ShortName = "PL",
                Action = Action.PickUp,
                PreState = TruckState.Chassis,
                PostState = TruckState.Loaded,
            };

        public static StopAction DropOffLoaded = new StopAction()
            {
                Id = 7,
                Name = "Drop Off Loaded",
                ShortName = "DL",
                Action = Action.DropOff,
                PreState = TruckState.Loaded,
                PostState = TruckState.Chassis,
            };

        public static StopAction PickupEmptyWithChassis = new StopAction()
            {
                Id = 8,
                Name = "Pickup Empty With Chassis",
                ShortName = "PEWC",
                Action = Action.PickUp,
                PreState = TruckState.Bobtail,
                PostState = TruckState.Empty,
            };

        public static StopAction DropOffEmptyWithChassis = new StopAction()
            {
                Id = 9,
                Name = "Drop Off Empty With Chassis",
                ShortName = "DEWC",
                Action = Action.DropOff,
                PreState = TruckState.Empty,
                PostState = TruckState.Bobtail,
            };

        public static StopAction PickupLoadedWithChassis = new StopAction()
            {
                Id = 10,
                Name = "Pickup Loaded With Chassis",
                ShortName = "PLWC",
                Action = Action.PickUp,
                PreState = TruckState.Bobtail,
                PostState = TruckState.Loaded,
            };

        public static StopAction DropOffLoadedWithChassis = new StopAction()
            {
                Id = 11,
                Name = "Drop Off Loaded With Chassis",
                ShortName = "DLWC",
                Action = Action.DropOff,
                PreState = TruckState.Loaded,
                PostState = TruckState.Bobtail,
            };

        public static StopAction LiveLoading = new StopAction()
            {
                Id = 12,
                Name = "Live Loading",
                ShortName = "LL",
                Action = Action.NoAction,
                PreState = TruckState.Empty | TruckState.Loaded,
                PostState = TruckState.Loaded,
            };

        public static StopAction LiveUnloading = new StopAction()
            {
                Id = 13,
                Name = "Live Unloading",
                ShortName = "LU",
                Action = Action.NoAction,
                PreState = TruckState.Empty | TruckState.Loaded,
                PostState = TruckState.Loaded | TruckState.Empty,
            };
    }
}