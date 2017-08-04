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
using PAI.FRATIS.SFL.Domain.Orders;

namespace PAI.FRATIS.SFL.Domain.Planning
{
    /// <summary>
    /// represents a plan
    /// </summary>
    public partial class Plan : EntitySubscriberBase, IDatedEntity
    {
        /// <summary>
        /// Gets or sets the unique id for a run
        /// </summary>
        public virtual int Run { get; set; }

        //public virtual int JobGroupId { get; set; }
        public virtual JobGroup JobGroup { get; set; }

        /// <summary>
        /// Gets or sets whether this Plan has been accepted
        /// A Plan is accepted once the Solution is displayed and the user
        /// selects to Transmit the solution to the Drivers
        /// This also triggers Terminal ETA planning messages
        /// </summary>
        public bool IsAccepted { get; set; }
        /// <summary>
        /// Gets or sets whether this plan is user created
        /// </summary>
        public virtual bool UserCreated { get; set; }

        /// <summary>
        /// Gets or sets whether this plan was modified
        /// </summary>
        public virtual bool UserModified { get; set; }

        /// <summary>
        /// Gets or sets whether this plan was transmitted
        /// </summary>
        public virtual bool Transmitted { get; set; }

        public int? PlanConfigId { get; set; }
        /// <summary>
        /// Gets or sets the PlanConfig
        /// </summary>
        public virtual PlanConfig PlanConfig { get; set; }

        /// <summary>
        /// Gets or sets the driver plans
        /// </summary>
        private ICollection<PlanDriver> _driverPlans = null;
        public virtual ICollection<PlanDriver> DriverPlans
        {
            get
            {
                return _driverPlans ?? (_driverPlans = new List<PlanDriver>());
            }
            set
            {
                _driverPlans = value;
            }
        }


        /// <summary>
        /// Gets or sets the unassigned jobs
        /// </summary>
        private ICollection<Job> _unassignedJobs = null;
        public virtual ICollection<Job> UnassignedJobs
        {
            get
            {
                return _unassignedJobs ?? (_unassignedJobs = new List<Job>());
            }
            set
            {
                _unassignedJobs = value;
            }
        }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public RouteSegmentMetric TotalMetrics
        {
            get
            {
                var result = new RouteSegmentMetric();
                if (DriverPlans != null)
                {
                    result = DriverPlans.Aggregate(result, (current, dp) => current + dp.TotalMetrics);
                }
                return result;
            }

        } 

        public int TotalJobCount
        {
            get
            {
                return TotalJobIds.Count();
            }
        }

        public IEnumerable<int> TotalJobIds
        {
            get
            {
                var ids = new HashSet<int>();
                foreach (var jobId in DriverPlans.SelectMany(dp => dp.JobPlans.Select(p => p.JobId).ToList()))
                {
                    ids.Add(jobId);
                }

                foreach (var j in UnassignedJobs)
                {
                    ids.Add(j.Id);
                }

                return ids.ToList();
            }
        }

    }
}
