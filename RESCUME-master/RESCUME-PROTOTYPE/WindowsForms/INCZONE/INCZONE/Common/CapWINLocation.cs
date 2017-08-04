using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCZONE.Common
{
    public class CapWINLocation
    {
        public string DisplayId { get; set; }
        public LocationType Location { get; set; }

        public CapWINLocation(string DisplayId, LocationType Location)
        {
            // TODO: Complete member initialization
            this.DisplayId = DisplayId;
            this.Location = Location;
        }
    }
}
