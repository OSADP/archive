using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCZONE.Common
{
    public class IncidentZone
    {
        public IncidentZone()
        {
            this.Lanes = new List<Lane>();
        }

        public List<Lane> Lanes { get; set; }
        public dateTime IncidentTime { get; set; }
        public int PostSpeed { get; set; }
    }
}
