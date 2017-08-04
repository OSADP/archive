using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.TravelerPortal.Models
{
    public class Leg
    {
        [Display(Name="Start Time")]
        public long startTime { get; set; }
        public long endTime { get; set; }
        public double distance { get; set; }
        public string mode { get; set; }
        public string route { get; set; }
        public string agencyName { get; set; }
        public string routeColor { get; set; }
        public int routeType { get; set; }
        public string routeId { get; set; }
        public string routeTextColor { get; set; }
        public string tripBlockId { get; set; }
        public string headsign { get; set; }
        public string agencyId { get; set; }
        public string tripId { get; set; }
        public From from { get; set; }
        public To to { get; set; }
        public string routeShortName { get; set; }
        public string routeLongName { get; set; }
        public int duration { get; set; }
        public EncodedPolylineBean legGeometry { get; set; }
        public List<CoordinateEntity> googlePoints { get; set; }
    }
}
