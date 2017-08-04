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

namespace PAI.FRATIS.Wrappers.WebFleet.Settings
{
    public class WebFleetSettings
    {
        public static string AccountName { get; set; }

        public static string UserName { get; set; }

        public static string Password { get; set; }

        public static string ApiKey { get; set; }

        static WebFleetSettings()
        {
            ApiKey = "1EA66960-0934-11E2-9C6C-FF3AAEFC9456";
            AccountName = "fec-miami";
            UserName = "integrator";
            Password = "#AKt7QRrGAeRcWS";
        }
    }
}
