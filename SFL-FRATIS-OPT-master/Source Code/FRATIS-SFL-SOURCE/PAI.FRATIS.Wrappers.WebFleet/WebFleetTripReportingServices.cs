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
using PAI.FRATIS.SFL.Services;
using PAI.FRATIS.Wrappers.WebFleet.Mapping;
using PAI.FRATIS.Wrappers.WebFleet.Model;
using PAI.FRATIS.Wrappers.WebFleet.TripReportingService;
using PAI.FRATIS.Wrappers.WebFleet.Settings;

namespace PAI.FRATIS.Wrappers.WebFleet
{
    public interface IWebFleetTripReportingService
    {
        ICollection<WebFleetStandStill> GetStandStills(SelectionTimeSpan dateRange, string objectNumber = "");
        //ICollection<WebFleetStandStill> GetStandStills(DateTime? startDate, DateTime? endDate, string objectNumber = "");
    }

    public class WebFleetTripReportingService : IWebFleetTripReportingService
    {
        private readonly IWebFleetMappingService _mappingService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public WebFleetTripReportingService(IWebFleetMappingService mappingService, IDateTimeHelper dateTimeHelper)
        {
            _mappingService = mappingService;
            _dateTimeHelper = dateTimeHelper;
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

        public ICollection<WebFleetStandStill> GetStandStills(SelectionTimeSpan dateRange, string objectNumber = "")
        {
            var result = new List<WebFleetStandStill>();
            var webService = new tripAndTimeReportingClient();

            var standStillParams = new StandStillReportParam();
            if (objectNumber.Length > 0)
            {
                standStillParams.@object = new ObjectIdentityParameter() {objectNo = objectNumber};
            }

            if (dateRange != SelectionTimeSpan.Unspecified)
            {
                standStillParams.dateRange = new DateRange()
                    {
                        rangePattern = ReportingServiceHelper.GetDateRangePattern(dateRange),
                        rangePatternSpecified = true
                    };
            }


            var response = webService.showStandStillReport(GetAuthenticationParameters(), GetGeneralParameters(), standStillParams);

            if (HandleResult(response))
            {
                result.AddRange(from StandStillList obj in response.results select _mappingService.Map(obj));
            }

            return result;
        }

        public ICollection<WebFleetStandStill> GetStandStills(DateTime? startDate, DateTime? endDate, string objectNumber = "")
        {
            throw new NotImplementedException();

            //var result = new List<WebFleetStandStill>();
            //var webService = new tripAndTimeReportingClient();

            //var standStillParams = new StandStillReportParam();
            //if (objectNumber.Length > 0)
            //{
            //    standStillParams.@object = new ObjectIdentityParameter() { objectNo = objectNumber };
            //}

            //if (startDate.HasValue && endDate.HasValue)
            //{
            //    //startDate = _dateTimeHelper.ConvertLocalToUtcTime(startDate.Value);
            //    //endDate = _dateTimeHelper.ConvertLocalToUtcTime(endDate.Value);

            //    standStillParams.dateRange = new DateRange()
            //        {
            //            rangePattern = new DateRangePattern(),
            //            rangePatternSpecified = false,
            //            from = startDate,
            //            fromSpecified = true,
            //            to = endDate,
            //            toSpecified = true,
            //        };
            //}

            //var response = webService.showStandStillReport(GetAuthenticationParameters(), GetGeneralParameters(), standStillParams);

            //if (HandleResult(response))
            //{
            //    result.AddRange(from StandStillList obj in response.results select _mappingService.Map(obj));
            //}

            //return result;
        }

    }
}
