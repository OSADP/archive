using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.TravelerPortal.Models
{
    public class To
    {
        public string name { get; set; }
        public StopId2 stopId { get; set; }
        public string stopCode { get; set; }
        public double lon { get; set; }
        public double lat { get; set; }
        public long arrival { get; set; }
        public long departure { get; set; }
        public object orig { get; set; }
        public object zoneId { get; set; }
        public int stopIndex { get; set; }

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
                return name;
        }

    }
}
