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
using PAI.FRATIS.SFL.Domain.Orders;

namespace PAI.FRATIS.SFL.Domain.Planning
{
    /// <summary>
    /// represents a PlanConfig
    /// </summary>
    public partial class PlanConfig : EntitySubscriberBase, IDatedEntity
    {
        private ICollection<Driver> _drivers = null;
        private ICollection<Job> _jobs = null;
        
        /// <summary>
        /// Gets or sets Due Date provided upon Plan Creation
        /// </summary>
        public virtual DateTime DueDate { get; set; }

        public virtual DateTime DueDateStart { get; set; }

        /// <summary>
        /// Gets or sets the default driver
        /// </summary>
        public virtual Driver DefaultDriver { get; set; }

        /// <summary>
        /// Gets or sets the drivers
        /// </summary>
        public virtual ICollection<Driver> Drivers
        {
            get { return _drivers ?? (_drivers = new List<Driver>()); }
            set { _drivers = value; }
        }

        public int? JobGroupId { get; set; }
        public virtual JobGroup JobGroup { get; set; }

        /// <summary>
        /// Gets or sets the jobs
        /// </summary>
        public virtual ICollection<Job> Jobs
        {
            get { return _jobs ?? (_jobs = new List<Job>()); }
            set { _jobs = value; }
        }

        public TimeSpan? ShiftStartTime { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
