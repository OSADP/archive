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

namespace PAI.FRATIS.Wrappers.WebFleet.Model
{
    public enum WebFleetOrderState : int
    {
        NotYetSent = 0,
        Sent = 100,
        Received = 101,
        Read = 102,
        Accepted = 103,
        ServiceOrderStarted = 201,
        ArrivedAtDestination = 202,
        WorkStarted = 203,
        WorkFinished = 204,
        DepartedFromDestination = 205,
        PickupOrderStarted = 221,
        ArrivedAtPickUpLocation = 222,
        PickUpStarted = 223,
        PickUpFinished = 224,
        DepartedFromPickUpLocation = 225,
        DeliveryOrderStarted = 241,
        ArrivedAtDeliveryLocation = 242,
        DeliveryStarted = 243,
        DeliveryFinished = 244,
        DepartedFromDeliveryLocation = 245,
        Resumed = 298,
        Suspended = 299,
        Cancelled = 301,
        Rejected = 302,
        Finished = 401
    }
}
