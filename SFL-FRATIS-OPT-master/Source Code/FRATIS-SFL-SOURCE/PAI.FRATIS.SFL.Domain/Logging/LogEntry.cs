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
using PAI.FRATIS.SFL.Domain.Users;

namespace PAI.FRATIS.SFL.Domain.Logging
{
    public class LogEntry : EntitySubscriberBase
    {
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public virtual LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// Gets or sets the full message
        /// </summary>
        public virtual string FullMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time of entry
        /// </summary>
        public virtual DateTime AuditDate { get; set; }

        /// <summary>
        /// Gets or sets the user
        /// </summary>
        public virtual User User { get; set; }
    }
}
