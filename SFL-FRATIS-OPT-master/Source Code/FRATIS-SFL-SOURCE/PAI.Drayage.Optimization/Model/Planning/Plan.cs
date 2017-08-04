using System;
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Planning
{
    /// <summary>
    /// represents a plan
    /// </summary>
    public partial class Plan : ModelBase
    {
        /// <summary>
        /// Gets or sets the driver plans
        /// </summary>
        private IList<PlanDriver> _driverPlans = null;
        public IList<PlanDriver> DriverPlans
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
        private IList<Job> _unassignedJobs = null;
        public virtual IList<Job> UnassignedJobs
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
    }
}
