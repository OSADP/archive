using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class EncodedPolylineBean
    {
        public string points { get; set; }
        public string levels { get; set; }
        public int length { get; set; }
    }
}
