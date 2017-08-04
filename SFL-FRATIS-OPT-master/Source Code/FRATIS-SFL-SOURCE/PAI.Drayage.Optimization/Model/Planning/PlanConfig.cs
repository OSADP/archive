using System;
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Planning
{
    /// <summary>
    /// Represents a plan configuration
    /// </summary>
    public partial class PlanConfig : ModelBase
    {
        private ICollection<Driver> _drivers = null;
        private ICollection<Job> _jobs = null;

        /// <summary>
        /// Gets or sets the default driver
        /// </summary>
        public Driver DefaultDriver { get; set; }

        /// <summary>
        /// Gets or sets the drivers
        /// </summary>
        public ICollection<Driver> Drivers
        {
            get { return _drivers ?? (_drivers = new List<Driver>()); }
            set { _drivers = value; }
        }

        /// <summary>
        /// Gets or sets the jobs
        /// </summary>
        public ICollection<Job> Jobs
        {
            get
            {
                return _jobs ?? (_jobs = new List<Job>());
            }
            set
            {
                _jobs = value;
            }
        }

    }
}
