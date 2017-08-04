using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
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

    }
}
