using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INCZONE.Model;

namespace INCZONE.Common
{
    public class VitalConfig
    {

        public VitalConfig(VehicleAlarm entity)
        {
            this.Id  = entity.Id;
            this.Persistance = entity.Persistance;
            this.Active = entity.Active;
        }

        public int Id { get; set; }
        public int Persistance { get; set; }
        public bool Active { get; set; }
    }
}
