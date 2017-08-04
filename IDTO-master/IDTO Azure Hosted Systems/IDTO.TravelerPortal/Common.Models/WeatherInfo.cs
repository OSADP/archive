using System;
using System.Collections.Generic;
using System.Linq;

namespace IDTO.TravelerPortal.Common.Models
{
    public class WeatherInfo
    {
        public string LocationName { get; set; }
        public double TemperatureDegF { get; set; }
        public String Conditions { get; set; }
        public DateTime LastUpdate { get; set; }
        public string IconURL { get; set; }
		public string IconName { get; set; }
    }
}