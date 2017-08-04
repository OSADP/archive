using System;
using System.Collections.Generic;
using System.Linq;

namespace PAI.Drayage.Optimization.Model.Equipment
{
    /// <summary>
    /// Represents a depot or station which 
    /// distributes/stores chassis and containers
    /// </summary>
    public partial class Depot : ModelBase
    {
        /// <summary>
        /// Gets or sets the Location
        /// </summary>
        public virtual Location Location { get; set; }
        
        /// <summary>
        /// Gets or sets the resources
        /// </summary>
        private ICollection<DepotEquipmentAvailability> _depotEquipmentAvailability = null;
        public virtual ICollection<DepotEquipmentAvailability> DepotEquipmentAvailability
        {
            get
            {
                return _depotEquipmentAvailability ?? (_depotEquipmentAvailability = new List<DepotEquipmentAvailability>());
            }
            set { _depotEquipmentAvailability = value; }
        }
    }
}
