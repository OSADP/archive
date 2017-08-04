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
using System.Text;
using System.Threading.Tasks;
using PAI.FRATIS.Wrappers.WebFleet.Model;
using PAI.FRATIS.Wrappers.WebFleet.OrdersService;

namespace PAI.FRATIS.Wrappers.WebFleet
{
    public static class OrdersServiceHelper
    {
        
        public static DateRangePattern GetDateRangePattern(SelectionTimeSpan timeSpanRange)
        {
            switch (timeSpanRange)
            {
                case SelectionTimeSpan.Today:
                    return DateRangePattern.D0;
                case SelectionTimeSpan.Yesterday:
                    return DateRangePattern.Dm1;
                case SelectionTimeSpan.ThisWeek:
                    return DateRangePattern.W0;
                case SelectionTimeSpan.LastWeek:
                    return DateRangePattern.Wm1;
                case SelectionTimeSpan.ThisMonth:
                    return DateRangePattern.M0;
                case SelectionTimeSpan.LastMonth:
                    return DateRangePattern.Mm1;
                case SelectionTimeSpan.ThisYear:
                    return DateRangePattern.Y0;
                default:
                    return DateRangePattern.D0; // default to today
            }
        }

    }
}
