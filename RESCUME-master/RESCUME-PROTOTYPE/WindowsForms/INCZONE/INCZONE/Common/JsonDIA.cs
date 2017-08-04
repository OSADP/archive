using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class JsonDIA
    {
        public int gpsfix { get; set; }
        public double heading { get; set; }
        public double speed { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double vdop { get; set; }
        public double hdop { get; set; }
        public int satinuse { get; set; }
        public string version { get; set; }
        public bool evaenabled { get; set; }
        public bool timenabled { get; set; }

        public override string ToString()
        {
            return string.Format("gpsfix : {0}, heading : {1}, speed : {2}, latitude : {3}, longitude : {4}, vdop : {5}, hdop : {6}, satinuse : {7}, version : {8}, evaenabled : {9}, timenabled : {10},", gpsfix, heading, speed, latitude, longitude, vdop, hdop, satinuse, version, evaenabled, timenabled);
        }
    }
}
