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

using PAI.FRATIS.Wrappers.WebFleet.DriverManagementService;
using PAI.FRATIS.Wrappers.WebFleet.Settings;

namespace PAI.FRATIS.Wrappers.WebFleet
{
    public interface IWebFleetDriverService
    {
        bool AddDriver(
            string name,
            string driverNo,
            string phone = "",
            string email = "",
            string code = "1234",
            string pin = "1234");

        bool DeleteDriver(string driverNumber);
    }

    public class WebFleetDriverService : IWebFleetDriverService
    {
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
            return result.statusCode == 0;      // TODO - Log Errors to Logger Service
        }

        public bool AddDriver(string name, string driverNo, string phone = "", string email = "", string code = "1234", string pin = "1234")
        {
            var webService = new driverManagementClient();
            var webFleetDriver = new InsertDriverParameter
            {
                name = name,
                code = driverNo,
                driverNo = driverNo,
                telMobile = phone,
                email = email,
                pin = pin
            };

            System.Threading.Thread.Sleep(15000);
            var response = webService.insertDriver(GetAuthenticationParameters(), GetGeneralParameters(), webFleetDriver);
            return HandleResult(response);
        }

        public bool DeleteDriver(string driverNumber)
        {
            var webService = new driverManagementClient();
            var webFleetDriver = new DeleteDriverParameter
            {
                driverNo = driverNumber
            };

            var response = webService.deleteDriver(GetAuthenticationParameters(), GetGeneralParameters(), webFleetDriver);

            return HandleResult(response);
        }

    }
}
