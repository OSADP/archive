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

namespace PAI.FRATIS.SFL.Domain.Logging
{
    public class SyncLogEntry : EntitySubscriberBase
    {
        public DateTime? TimeStamp { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public int UpdatedJobCount { get; set; }

        public int CreatedJobCount { get; set; }

        public int ExistingJobCount { get; set; }

        public int LocationErrorCount { get; set; }

        public int RouteStopsRecreatedCount { get; set; }
        
        public int JobErrorCount { get; set; }

        public List<string> Errors { get; set; } 
    }
}