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
    [Flags]
    public enum TruckState
    {
        Invalid = 0,
        Bobtail = 0x2,
        Chassis = 0x4,
        Empty = 0x8,
        Loaded = 0x20,
        Any = Bobtail | Chassis | Empty | Loaded
    }
}