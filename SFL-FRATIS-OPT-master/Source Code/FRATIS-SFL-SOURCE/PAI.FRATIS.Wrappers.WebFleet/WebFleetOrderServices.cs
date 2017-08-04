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
using PAI.FRATIS.Wrappers.WebFleet.Mapping;
using PAI.FRATIS.Wrappers.WebFleet.Model;
using PAI.FRATIS.Wrappers.WebFleet.OrdersService;
using PAI.FRATIS.Wrappers.WebFleet.Settings;

namespace PAI.FRATIS.Wrappers.WebFleet
{
    public interface IWebFleetOrderService
    {
        WebFleetOrder GetOrder(string orderNumber);
        ICollection<WebFleetOrder> GetOrders(SelectionTimeSpan dateRange);
        ICollection<WebFleetOrder> GetOrders(int selectionDateRangeInt);
        bool ClearOrders(string objectNumber, bool markDeleted);
        bool DeleteOrder(string orderNumber, bool markDeleted);
        bool ReassignOrder(string orderNumber, string assignToObjectNumber);
        bool AssignOrder(string orderNumber, string assignToObjectNumber);
        bool SendOrder(string orderNumber, string assignToObjectNumber, bool sendToDevice, bool autoStartJob, string orderText = "");
        bool InsertOrder(string orderNumber, string assignToObjectNumber, string orderText = "", string webfleetLocationId = "", DateTime? arrivalTime = null);

        bool IsOrderFinished(IEnumerable<WebFleetOrder> orders);
    }

    public class WebFleetOrderService : IWebFleetOrderService
    {
        private readonly IWebFleetMappingService _mappingService;

        public WebFleetOrderService(IWebFleetMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        public AuthenticationParameters GetAuthenticationParameters()
        {
            var auth = new AuthenticationParameters()
            {
                accountName = WebFleetSettings.AccountName,
                userName = WebFleetSettings.UserName,
                password = WebFleetSettings.Password,
                apiKey = WebFleetSettings.ApiKey
            };

            return auth;
        }

        public GeneralParameters GetGeneralParameters()
        {
            return new GeneralParameters
            {
                locale = KnownLocales.US,
                timeZone = KnownTimeZones.America_New_York
            };
        }

        public bool HandleResult(ServiceOpResult result)
        {
            return result.statusCode == 0;
            // TODO - Log Errors to Logger Service
        }

        public WebFleetOrder GetOrder(string orderNumber)
        {
            var result = new List<WebFleetOrder>();
            var webService = new WebFleet.OrdersService.ordersClient();
            var ordersParameters = new OrderReportParameters() {orderNo = orderNumber};
            var response = webService.showOrderReport(GetAuthenticationParameters(), GetGeneralParameters(),
                                                      ordersParameters);

            if (HandleResult(response))
            {
                result.AddRange(from ReportedOrderData order in response.results select _mappingService.Map(order));
            }

            return result.Count > 0 ? result.FirstOrDefault() : null;
        }

        public bool IsOrderFinished(IEnumerable<WebFleetOrder> orders)
        {
            return orders.All(order => order.OrderState == WebFleetOrderState.Finished);
        }

        public ICollection<WebFleetOrder> GetOrders(int selectionDateRangeInt)
        {
            return GetOrders((SelectionTimeSpan) selectionDateRangeInt);
        }

        public ICollection<WebFleetOrder> GetOrders(SelectionTimeSpan dateRange)
        {
            return GetOrders(OrdersServiceHelper.GetDateRangePattern(dateRange));
        }

        public ICollection<WebFleetOrder> GetOrders(DateRangePattern dateRangePattern)
        {
            var result = new List<WebFleetOrder>();

            var webService = new WebFleet.OrdersService.ordersClient();

            var ordersParameters = new OrderReportParameters
                {
                    dateRange = new DateRange()
                        {
                            rangePatternSpecified = true,
                            rangePattern = dateRangePattern,
                            fromSpecified = false,
                            toSpecified = false,
                        }
                };

            var response = webService.showOrderReport(GetAuthenticationParameters(), GetGeneralParameters(),
                                                      ordersParameters);

            if (HandleResult(response))
            {
                result.AddRange(from ReportedOrderData order in response.results select _mappingService.Map(order));
            }

            return result;
        }

        /// <summary>
        /// Removes all orders from the device and optionally 
        /// marks them as deleted in WebFleet
        /// </summary>
        /// <param name="objectNumber"></param>
        /// <param name="markDeleted"></param>
        /// <returns></returns>
        public bool ClearOrders(string objectNumber, bool markDeleted)
        {
            var webService = new ordersClient();
            var response = webService.clearOrders(GetAuthenticationParameters(), GetGeneralParameters(),
                                                  new ClearOrdersParameter()
                                                      {
                                                          objectNo = objectNumber,
                                                          markDeleted = markDeleted,
                                                          markDeletedSpecified = true
                                                      });
            return HandleResult(response);
        }

        public bool DeleteOrder(string orderNumber, bool markDeleted)
        {
            var webService = new ordersClient();
            var response = webService.deleteOrder(GetAuthenticationParameters(), GetGeneralParameters(),
                                                  new DeleteOrderParameter()
                                                      {
                                                          markDeleted = markDeleted,
                                                          markDeletedSpecified = true,
                                                          orderNo = orderNumber
                                                      });
            return HandleResult(response);
        }

        public bool ReassignOrder(string orderNumber, string assignToObjectNumber)
        {
            var webService = new ordersClient();
            var response = webService.reassignOrder(GetAuthenticationParameters(), GetGeneralParameters(),
                                                    new OrderIdentityParameter() {orderNo = orderNumber},
                                                    new ObjectIdentityParameter() {objectNo = assignToObjectNumber},
                                                    new AdvancedReAssignOrderParameter());
            return HandleResult(response);
        }

        public bool AssignOrder(string orderNumber, string assignToObjectNumber)
        {
            var webService = new ordersClient();
            var response = webService.assignOrder(GetAuthenticationParameters(), GetGeneralParameters(),
                                                  new AssignOrderParameter()
                                                      {
                                                          orderNo = orderNumber,
                                                          objectNo = assignToObjectNumber
                                                      },
                                                  new AdvancedAssignOrderParameter());
            return HandleResult(response);
        }


        /// <summary>
        /// Inserts and sends an order to WebFleet, and dispatches the order
        /// to the target object within WebFleet
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="assignToObjectNumber"></param>
        /// <param name="orderText"></param>
        /// <returns></returns>
        public bool SendOrder(string orderNumber, string assignToObjectNumber, bool sendToDevice, bool autoStartJob, string orderText = "")
        {
            var webService = new ordersClient();

            var automations = new List<OrderAutomation?>();
            if (autoStartJob)
            {
                automations = new List<OrderAutomation?>()
                                {
                    OrderAutomation.START,
                    OrderAutomation.ACCEPT,
                    OrderAutomation.NAVIGATING_TO,
                };
            }

            var codes = new List<OrderStateCode?>();
            var response = webService.sendOrder(GetAuthenticationParameters(), GetGeneralParameters(),
                                                new BasicOrderParameter()
                                                    {
                                                        orderNo = orderNumber, 
                                                        orderText = orderText
                                                    },
                                                new ObjectIdentityParameter()
                                                    {
                                                        objectNo = assignToObjectNumber,
                                                    },
                                                new AdvancedSendOrderParameter()
                                                    {
                                                        orderAutomations = automations.ToArray(), 
                                                        stateCode = codes.ToArray()
                                                    });
            return HandleResult(response);
        }

        /// <summary>
        /// Inserts an order to WebFleet, but does not send the order message
        /// and must manually be dispatched to an object within WebFleet
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="assignToObjectNumber"></param>
        /// <param name="orderText"></param>
        /// <param name="scheduledArrivalTime"> </param>
        /// <returns></returns>
        public bool InsertOrder(string orderNumber, string assignToObjectNumber, string orderText = "", string webfleetLocationId = "",
            DateTime? scheduledArrivalTime = null)
        {
            if (string.IsNullOrEmpty(assignToObjectNumber)) return false;
            var webService = new ordersClient();
            var destOrder = new DestinationOrderParameter()
                { addrNoToUseAsDestination = webfleetLocationId, orderNo = orderNumber, orderText = orderText };

            if (scheduledArrivalTime.HasValue)
            {
                destOrder.scheduledCompletionDateAndTimeSpecified = true;
                destOrder.scheduledCompletionDateAndTime = scheduledArrivalTime.Value.AddHours(6); // todo datetime dynamic helper
            }
            else
            {
                // set local time
            }

            var resp = webService.insertDestinationOrder(
                GetAuthenticationParameters(),
                GetGeneralParameters(),
                destOrder,
                new ObjectIdentityParameter()
                    {
                        objectNo = assignToObjectNumber
                    },
                new AdvancedInsertOrderParameter());

            if (resp.statusCode != 0)
            {
                resp = webService.insertDestinationOrder(
                GetAuthenticationParameters(),
                GetGeneralParameters(),
                destOrder,
                new ObjectIdentityParameter(){},
                new AdvancedInsertOrderParameter()
                    {
                        
                    });
            }

            if (resp.statusCode != 0)
            {

                var errorStatus = resp.statusCode;  // something went wrong, log todo

            }

            return HandleResult(resp);
        }
    }
}