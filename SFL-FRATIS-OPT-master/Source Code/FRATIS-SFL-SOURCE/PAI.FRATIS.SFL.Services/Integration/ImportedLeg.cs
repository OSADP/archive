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

namespace PAI.FRATIS.SFL.Services.Integration
{
    public class ImportedLeg
    {
        public int LoadTime { get; set; }
        public string ManifestNumber { get; set; }
        public string LegNumber { get; set; }
        public string ManifestType { get; set; }
        public string LegType { get; set; }
        public int NumberOfLegs { get; set; }
        public string LegNumberCredited { get; set; }
        public int SequenceNumber { get; set; }
        public string RecordType { get; set; }
        public bool ScheduledStop { get; set; }
        public string StopOffCity { get; set; }
        public string StopOffState { get; set; }

        public string OriginZone { get; set; }
        public string DestinationZone { get; set; }
        

        // customer properties
        public string CustomerNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyState { get; set; }
        public string CompanyZip { get; set; }

        /// <summary>
        /// Int Representation of Zip Code (first 5 characters)
        /// </summary>
        public int CompanyZipInt
        {
            get
            {
                int zip = 0;
                if (!string.IsNullOrEmpty(CompanyZip) && CompanyZip.Length > 5)
                {
                    Int32.TryParse(CompanyZip.Substring(0, 5), out zip);
                }
                else
                {
                    Int32.TryParse(CompanyZip, out zip);
                }
                return zip;
            }
        }
        
        // consignee properties
        public int ConsigneeNumber { get; set; }
        public string ConsigneeName { get; set; }
        
        // shipper properties
        public int ShipperNumber { get; set; }
        public string ShipperName { get; set; }
        
        // order details
        public string BillOfLadingNumber { get; set; }
        public string SealNumber { get; set; }
        public string ServiceType { get; set; }

        public string OrderOriginCity { get; set; }
        public string OrderOriginState { get; set; }
        public string OrderOriginZip { get; set; }
        public string OrderDestinationCity { get; set; }
        public string OrderDestinationState { get; set; }
        public string OrderDestinationZip { get; set; }

        public DateTime LoadDateTime { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public DateTime DispatchDateTime { get; set; }
        public DateTime ScheduledDateTime { get; set; }

        public string LegOriginCity { get; set; }
        public string LegOriginState { get; set; }
        public string LegOriginZip { get; set; }
        
        public string LegDestinationCity { get; set; }
        public string LegDestinationState { get; set; }
        public string LegDestinationZip { get; set; }
        public string Trailer { get; set; }
        public bool IsHazmat { get; set; }
    }
}