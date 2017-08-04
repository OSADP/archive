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

namespace PAI.FRATIS.Wrappers.WebFleet.Model
{
    public class WebFleetOrder
    {
        private string _orderNumber;

        public WebFleetOrder()
        {
            OrderNumberDetails = new OrderNumberDetails();
        }

        private OrderNumberDetails GetOrderNumberDetails(string orderNumber)
        {
            var arrOrderId = orderNumber.Split('-');
            string truncatedOrderNumber = string.Empty;
            for (int i = 0; i < arrOrderId.Length - 1; i++)
            {
                if (i > 0)
                {
                    truncatedOrderNumber += "-";
                }
                truncatedOrderNumber += arrOrderId[i];
            }

            return new OrderNumberDetails()
                {
                    OrderNumber = truncatedOrderNumber,
                    RouteStop = arrOrderId.Length > 1 ? arrOrderId[arrOrderId.Length - 1] : string.Empty
                };
        }
        
        public string OrderNumber
        {
            get
            {
                return _orderNumber;
            }
            set
            {
                _orderNumber = value;
                OrderNumberDetails = GetOrderNumberDetails(_orderNumber);
            }
        }

        public string ObjectNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public WebFleetOrderType OrderType { get; set; }
        public int OrderTypeInt
        {
            get { return (int) OrderType; }
        }

        public WebFleetOrderState OrderState { get; set; }

        public string OrderText { get; set; }
        
        public string DriverNumber { get; set; }
        public string DriverName { get; set; }
        
        public WebFleetPosition OrderPosition { get; set; }
        public WebFleetAddress OrderAddress { get; set; }

        public OrderNumberDetails OrderNumberDetails { get; set; }
    }
}