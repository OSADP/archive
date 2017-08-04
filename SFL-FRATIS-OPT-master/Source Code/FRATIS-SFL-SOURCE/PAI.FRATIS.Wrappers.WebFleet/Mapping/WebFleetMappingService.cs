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

using PAI.FRATIS.Wrappers.WebFleet.MessagesService;
using PAI.FRATIS.Wrappers.WebFleet.ReportingService;
using PAI.FRATIS.Wrappers.WebFleet.AddressService;
using PAI.FRATIS.Wrappers.WebFleet.MessagesService;
using PAI.FRATIS.Wrappers.WebFleet.Model;

using Address = PAI.FRATIS.Wrappers.WebFleet.AddressService.Address;
using OrderType = PAI.FRATIS.Wrappers.WebFleet.OrdersService.OrderType;
using ReportedOrderData = PAI.FRATIS.Wrappers.WebFleet.OrdersService.ReportedOrderData;
using ObjectReport = PAI.FRATIS.Wrappers.WebFleet.ReportingService.ObjectReport;
using OrderStateCode = PAI.FRATIS.Wrappers.WebFleet.OrdersService.OrderStateCode;
using RoutingData = PAI.FRATIS.Wrappers.WebFleet.AddressService.RoutingData;
using WorkState = PAI.FRATIS.Wrappers.WebFleet.ReportingService.WorkState;
using StandStillList = PAI.FRATIS.Wrappers.WebFleet.TripReportingService.StandStillList; 

namespace PAI.FRATIS.Wrappers.WebFleet.Mapping
{
    public interface IWebFleetMappingService
    {
        WebFleetMessage Map(MessageTO message);
        WebFleetMessage Map(QueueServiceData msg);
        WebFleetObject Map(ObjectReport obj);
        WebFleetOrder Map(ReportedOrderData order);
        WebFleetDriver Map(DriverReport driver);
        WebFleetPosition Map(AddressService.CompleteLocationWithAdditionalInformation location);
        WebFleetRouteEstimate Map(RoutingData routingData);
        WebFleetStandStill Map(StandStillList standStill);
        WebFleetAddress Map(AddressService.Address address);
    }

    /// <summary>The web fleet mapping service.</summary>
    public class WebFleetMappingService : IWebFleetMappingService
    {
        public WebFleetMessage Map(MessageTO msg)
        {
            var result = new WebFleetMessage()
            {
                MessageId = msg.messageId,
                MessageTime = msg.messageTime,
                MessageText = msg.messageText,
                PositionText = msg.positionText,
                ObjectNumber = msg.@object.objectNo,
            };

            SetMessageStatusFromText(result);

            return result;
        }

        /// <summary>
        /// Analyzes the MessageText property of the provided WebFleetMessage
        /// and sets the MessageStatus, InformationalStatus, and InformationPosition
        /// information based upon the status string
        /// </summary>
        /// <param name="messageObject"></param>
        private void SetMessageStatusFromText(WebFleetMessage messageObject)
        {
            var messageText = messageObject.MessageText;
            if (messageText.StartsWith("Message \""))
            {
                if (messageText.EndsWith("read"))
                {
                    messageObject.Status = MessageStatus.Read;
                    messageObject.MessageText = messageText.Substring(9, messageText.Length - (9 + 6));
                }
                else if (messageText.EndsWith("received"))
                {
                    messageObject.Status = MessageStatus.Received;
                    messageObject.MessageText = messageText.Substring(9, messageText.Length - (9 + 10));
                }
            }
            else if (messageText.Length > 17)
            {
                switch (messageText.Substring(0, 18).ToLower())
                {
                    case "destination reache":
                        
                        messageObject.Status = MessageStatus.Informational;
                        messageObject.InformationalStatusType = InformationalStatusType.DestinationReached;
                        messageObject.InformationStatusPositionText = GetParenthesisText(messageText);
                        messageObject.MessageText = "Destination reached";
                        break;
                    case "navigation cancell":
                        messageObject.Status = MessageStatus.Informational;
                        messageObject.InformationalStatusType = InformationalStatusType.NavigationCancelled;
                        messageObject.InformationStatusPositionText = GetParenthesisText(messageText);
                        messageObject.MessageText = "Navigation cancelled";
                        break;
                    case "navigation started":
                        messageObject.Status = MessageStatus.Informational;
                        messageObject.InformationalStatusType = InformationalStatusType.NavigationStarted;
                        messageObject.InformationStatusPositionText = GetParenthesisText(messageText);
                        messageObject.MessageText = "Navigation started";
                        break;
                }
            }
        }
    
        /// <summary>
        /// Analyzes the provides string for opening and closing parenthesis
        /// and returns the string in between the parenthesis characters
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string GetParenthesisText(string message)
        {
            var result = "";
            var startPos = message.IndexOf("(", System.StringComparison.Ordinal);
            var endPos = message.IndexOf(")", System.StringComparison.Ordinal);

            if (startPos >= 0 && endPos >= 0)
            {
                try
                {
                    result = message.Substring(startPos + 1, endPos - startPos - 1);
                }
                catch
                {
                    result = "";
                }
            }

            return result;
        }
            
        public WebFleetMessage Map(QueueServiceData msg)
        {
            var result = new WebFleetMessage()
            {
                MessageId = msg.msgid.messageId,
                MessageTime = msg.msgTime,
                MessageText = msg.msgText,
                PositionText = msg.posText,
                ObjectNumber = msg.objectNo.objectNo,
            };

            SetMessageStatusFromText(result);

            return result;
        }

        public WebFleetObject Map(ObjectReport obj)
        {
            var result = new WebFleetObject()
                {
                    ObjectNumber = obj.objectNo,
                    ObjectTypeString = obj.objectType.Replace("_", ""),
                    Odometer = obj.odometer,
                    Description = obj.description,
                    PositionText = obj.positionText,
                    Position = new WebFleetPosition()
                        {
                            TimeStamp = DateTime.UtcNow,
                            LatitudeInt = obj.position.latitude,
                            LongitudeInt = obj.position.longitude,
                            PositionMovement = new WebFleetPositionMovement()
                                {
                                    Course = obj.course
                                }   
                        },
                };

            if (obj.navDestination != null)
            {
                result.DestinationPosition = new WebFleetPosition()
                    {
                        LatitudeInt = obj.navDestination.navTargetLatitude,
                        LongitudeInt = obj.navDestination.navTargetLongitude,
                        PositionText = obj.navDestination.navDestination
                    };
 
                result.DestinationEstimate = new WebFleetDestinationEstimate()
                {
                    ArrivalTime = obj.navDestination.navEtaTime,
                    Distance = obj.navDestination.distanceToDestination
                };
            }

            return result;
        }

        private int GetOrderStateInt(OrdersService.OrderState orderState)
        {
            var result = 0;
            if (orderState != null)
            {
                switch (orderState.stateCode)
                {
                    case OrderStateCode.NotYetSent:
                        result = (int) WebFleetOrderState.NotYetSent;
                        break;
                    case OrderStateCode.Sent:
                        result = (int)WebFleetOrderState.Sent;
                        break;
                    case OrderStateCode.Received:
                        result = (int) WebFleetOrderState.Received;
                        break;
                    case OrderStateCode.Read:
                        result = (int)WebFleetOrderState.Read;
                        break;
                    case OrderStateCode.Accepted:
                        result = (int)WebFleetOrderState.Accepted;
                        break;
                    case OrderStateCode.ServiceOrderStarted:
                        result = (int) WebFleetOrderState.ServiceOrderStarted;
                        break;
                    case OrderStateCode.ArrivedAtDestination:
                        result = (int) WebFleetOrderState.ArrivedAtDestination;
                        break;
                    case OrderStateCode.WorkStarted:
                        result = (int)WebFleetOrderState.WorkStarted;
                        break;
                    case OrderStateCode.WorkFinished:
                        result = (int) WebFleetOrderState.WorkFinished;
                        break;
                    case OrderStateCode.DepartedFromDestination:
                        result = (int) WebFleetOrderState.DepartedFromDestination;
                        break;
                    case OrderStateCode.PickupOrderStarted:
                        result = (int) WebFleetOrderState.PickupOrderStarted;
                        break;
                    case OrderStateCode.ArrivedAtPickUpLocation:
                        result = (int) WebFleetOrderState.ArrivedAtPickUpLocation;
                        break;
                    case OrderStateCode.PickUpStarted:
                        result = (int)WebFleetOrderState.PickUpStarted;
                        break;
                    case OrderStateCode.PickUpFinished:
                        result = (int) WebFleetOrderState.PickupOrderStarted;
                        break;
                    case OrderStateCode.DepartedFromPickUpLocation:
                        result = (int) WebFleetOrderState.DepartedFromPickUpLocation;
                        break;
                    case OrderStateCode.DeliveryOrderStarted:
                        result = (int) WebFleetOrderState.DeliveryOrderStarted;
                        break;
                    case OrderStateCode.ArrivedAtDeliveryLocation:
                        result = (int) WebFleetOrderState.ArrivedAtDeliveryLocation;
                        break;
                    case OrderStateCode.DeliveryStarted:
                        result = (int) WebFleetOrderState.DeliveryStarted;
                        break;
                    case OrderStateCode.DeliveryFinished:
                        result = (int) WebFleetOrderState.DeliveryFinished;
                        break;
                    case OrderStateCode.DepartedFromDeliveryLocation:
                        result = (int)WebFleetOrderState.DepartedFromDeliveryLocation;
                        break;
                    case OrderStateCode.Resumed:
                        result = (int) WebFleetOrderState.Resumed;
                        break;
                    case OrderStateCode.Suspended:
                        result = (int) WebFleetOrderState.Suspended;
                        break;
                    case OrderStateCode.Cancelled:
                        result = (int) WebFleetOrderState.Cancelled;
                        break;
                    case OrderStateCode.Rejected:
                        result = (int) WebFleetOrderState.Rejected;
                        break;
                    case OrderStateCode.Finished:
                        result = (int) WebFleetOrderState.Finished;
                        break;
                }
            }
            return result;
        }

        private WebFleetOrderState GetOrderState(OrdersService.OrderState orderState)
        {
            return (WebFleetOrderState) GetOrderStateInt(orderState);
        }

        public WebFleetOrder Map(ReportedOrderData order)
        {
            var result = new WebFleetOrder()
            {
                OrderNumber = order.orderId,
                OrderState =  GetOrderState(order.orderState),
                ObjectNumber = order.@object != null && order.@object.objectNo != null ? order.@object.objectNo : string.Empty,
                OrderDate = order.orderDate ?? new DateTime?(),
                DriverNumber = order.driverNo ?? string.Empty,
                OrderText = order.orderText ?? string.Empty,
                OrderPosition = new WebFleetPosition()
                    {
                        Latitude = order.geoPosition.latitude,
                        Longitude = order.geoPosition.longitude,                        
                    },
                OrderAddress = new WebFleetAddress()
                    {
                        StreetAddress = string.Format("{0} {1}", order.orderAddress.addrNr, order.orderAddress.street).Trim(),
                        City = order.orderAddress.city,
                        Zip = order.orderAddress.zip
                    }
            };

            if (order.orderTypeSpecified && order.orderType.HasValue)
            {
                switch (order.orderType.Value)
                {
                    case OrderType.DELIVERY_ORDER:
                        result.OrderType = WebFleetOrderType.DeliveryOrder;
                        break;
                    case OrderType.PICKUP_ORDER:
                        result.OrderType = WebFleetOrderType.PickupOrder;
                        break;
                    case OrderType.SERVICE_ORDER:
                        result.OrderType = WebFleetOrderType.ServiceOrder;
                        break;
                }


                if (order.orderState != null)
                {
                    switch (order.orderState.stateCode)
                    {
                        case OrderStateCode.Accepted:
                            result.OrderState = WebFleetOrderState.Accepted;
                            break;
                        case OrderStateCode.ArrivedAtDeliveryLocation:
                            result.OrderState = WebFleetOrderState.ArrivedAtDeliveryLocation;
                            break;
                        case OrderStateCode.ArrivedAtPickUpLocation:
                            result.OrderState = WebFleetOrderState.ArrivedAtPickUpLocation;
                            break;
                        case OrderStateCode.ArrivedAtDestination:
                            result.OrderState = WebFleetOrderState.ArrivedAtDestination;
                            break;
                        case OrderStateCode.Cancelled:
                            result.OrderState = WebFleetOrderState.Cancelled;
                            break;
                        case OrderStateCode.DeliveryFinished:
                            result.OrderState = WebFleetOrderState.DeliveryFinished;
                            break;
                        case OrderStateCode.DeliveryOrderStarted:
                            result.OrderState = WebFleetOrderState.DeliveryOrderStarted;
                            break;
                        case OrderStateCode.DeliveryStarted:
                            result.OrderState = WebFleetOrderState.DeliveryStarted;
                            break;
                        case OrderStateCode.Finished:
                            result.OrderState = WebFleetOrderState.Finished;
                            break;
                        case OrderStateCode.NotYetSent:
                            result.OrderState = WebFleetOrderState.NotYetSent;
                            break;
                        case OrderStateCode.Read:
                            result.OrderState = WebFleetOrderState.Read;
                            break;
                        case OrderStateCode.Received:
                            result.OrderState = WebFleetOrderState.Received;
                            break;
                        case OrderStateCode.Rejected:
                            result.OrderState = WebFleetOrderState.Rejected;
                            break;
                        case OrderStateCode.Resumed:
                            result.OrderState = WebFleetOrderState.Resumed;
                            break;
                        case OrderStateCode.Sent:
                            result.OrderState = WebFleetOrderState.Sent;
                            break;
                        case OrderStateCode.ServiceOrderStarted:
                            result.OrderState = WebFleetOrderState.ServiceOrderStarted;
                            break;
                        case OrderStateCode.Suspended:
                            result.OrderState = WebFleetOrderState.Suspended;
                            break;
                        case OrderStateCode.WorkFinished:
                            result.OrderState = WebFleetOrderState.WorkFinished;
                            break;
                        case OrderStateCode.WorkStarted:
                            result.OrderState = WebFleetOrderState.WorkStarted;
                            break;
                    }
                }

            }
            return result;
        }

        public WebFleetDriver Map(DriverReport driver)
        {
            var result = new WebFleetDriver()
                {
                    Name = driver.personName.name1,
                    Company = driver.company,
                    DriverNumber = driver.driverNo,
                    Email = driver.email,
                    Pin = driver.pin,
                    Code = driver.driverCode,
                };

            if (driver.currentVehicle != null)
                result.CurrentVehicleObjectNumber = driver.currentVehicle.objectNo;
            
            if (driver.addrPos != null && driver.addrPos.latitudeSpecified && driver.addrPos.longitudeSpecified)
            {
                result.CurrentPosition = new WebFleetPosition()
                    {
                        Latitude = driver.addrPos.latitude,
                        Longitude = driver.addrPos.longitude,
                    };

                if (driver.location != null)
                {
                    result.CurrentPosition.PositionText = string.Format("{0} {1} {2}",
                                                                        driver.location.street, driver.location.city,
                                                                        driver.location.postcode);
                }
            }

            if (driver.phoneNumbers != null)
            {
                if (driver.phoneNumbers.phoneMobile != null && driver.phoneNumbers.phoneMobile.Length > 2)
                    result.Phone = string.Format("{0} (Mobile)", driver.phoneNumbers.phoneMobile);
                else if (driver.phoneNumbers.phonePersonal != null && driver.phoneNumbers.phonePersonal.Length > 2)
                    result.Phone = string.Format("{0} (Personal)", driver.phoneNumbers.phonePersonal);
                else if (driver.phoneNumbers.phoneBusiness != null && driver.phoneNumbers.phoneBusiness.Length > 2)
                    result.Phone = string.Format("{0} (Business)", driver.phoneNumbers.phoneBusiness);
            }

            if (driver.currentWorkStateSpecified && driver.currentWorkState != null)
            {
                result.WorkInfo = new WebFleetDriverWorkInfo();

                switch (driver.currentWorkState)
                {
                    case WorkState.DRIVING:
                        result.WorkInfo.Status = WebFleetWorkState.Driving;
                        break;
                    case WorkState.FREETIME:
                        result.WorkInfo.Status = WebFleetWorkState.FreeTime;
                        break;
                    case WorkState.OTHER_WORK:
                        result.WorkInfo.Status = WebFleetWorkState.Other;
                        break;
                    case WorkState.PAUSE:
                        result.WorkInfo.Status = WebFleetWorkState.Pause;
                        break;
                    case WorkState.STANDBY:
                        result.WorkInfo.Status = WebFleetWorkState.Unknown;
                        break;
                    case WorkState.UNKNOWN:
                        result.WorkInfo.Status = WebFleetWorkState.Unknown;
                        break;
                    case WorkState.WORKING:
                        result.WorkInfo.Status = WebFleetWorkState.Working;
                        break;
                }


                result.WorkInfo.StartTime = driver.currentWorkingTimeStart;
                result.WorkInfo.EndTime = driver.currentWorkingTimeEnd;
                
                if (driver.currentWorkingTime != null)
                    result.WorkInfo.WorkTimeSpan = new TimeSpan(driver.currentWorkingTime.Value);

            }
            return result;
        }

        public WebFleetPosition Map(AddressService.CompleteLocationWithAdditionalInformation location)
        {
            var result = new WebFleetPosition()
            {
                TimeStamp = DateTime.UtcNow,
                PositionText = location.positionText,
                Latitude = location.geoPosition.latitude * .000001,
                Longitude = location.geoPosition.longitude * .000001,
            };

            return result;
        }

        public WebFleetRouteEstimate Map(RoutingData routingData)
        {
            var result = new WebFleetRouteEstimate()
                {
                    ArrivalDateTime = routingData.endDateTime,
                    DelaySeconds = routingData.delay,
                    Distance = new WebFleetDistance(routingData.distance.ToString()),
                    TripDuration = new TimeSpan(0, 0, routingData.time)
                };
            return result;
        }

        public WebFleetStandStill Map(StandStillList standStill)
        {
            var result = new WebFleetStandStill()
            {
                StartTime = standStill.startTime,
                EndTime = standStill.endTime,
                ObjectNumber = standStill.@object.objectNo,
                Position = new WebFleetPosition()
                    {
                        Latitude = standStill.position.latitude,
                        Longitude = standStill.position.longitude,
                        PositionText = standStill.posText
                    }
            };
            return result;
        }

        public WebFleetAddress Map(Address address)
        {
            var result = new WebFleetAddress()
                             {                          
                                 WebFleetId = address.addressNo,
                                 DisplayName = address.name1,
                                 Name2 = address.name2,
                                 Name3 = address.name3,
                                 StreetAddress = address.location.street,
                                 City = address.location.city,
                                 State = address.location.addrRegion,
                                 Zip = address.location.postcode,
                                 Phone = address.contact.phoneMobile,
                                 Email = address.contact.emailAddress,
                                 ContactName = address.contact.contactName,
                                 LatitudeInt = address.location.geoPosition.latitude,
                                 LongitudeInt = address.location.geoPosition.longitude,
                             };
            return result;
        }
    }
}
