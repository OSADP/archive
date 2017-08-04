using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.TravelerPortal.Models
{
    public class From
    {
        public string name { get; set; }
        public StopId stopId { get; set; }
        public string stopCode { get; set; }
        public double lon { get; set; }
        public double lat { get; set; }
        public object arrival { get; set; }
        public object departure { get; set; }
        public string orig { get; set; }
        public object zoneId { get; set; }
        public object stopIndex { get; set; }

        public string GetName()
        {
            if (name.Length >= 4)
            {
                if (name.Substring(0, 4).Equals("way "))
                {
                    return "Unnamed Road";
                }
                else
                {
                    return name;
                }
            }
            else
            {
                return name;
            }
        }

    }
}
