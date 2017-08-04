using System;
using System.Linq;

namespace PAI.Drayage.Optimization.Model.Equipment
{
    public class DepotEquipmentAvailability : ModelBase
    {
        /// <summary>
        /// Gets or sets the Depot
        /// </summary>
        public Depot Depot { get; set; }

        /// <summary>
        /// Gets or sets the equipment configuration
        /// </summary>
        public EquipmentConfiguration EquipmentConfiguration { get; set; }
    }
}
