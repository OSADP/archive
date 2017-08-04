using System.Globalization;
using System.Threading;
using System.IO;
using System;

namespace INCZONE.NMEA
{

    public class GpsHandler
    {
        internal static NumberFormatInfo NumberFormatEnUs = new CultureInfo("en-US", false).NumberFormat;

        /// <summary>
        /// Initializes a GpsHandler for communication with GPS receiver.
        /// The GpsHandler is used for communication with the GPS device and process information from the GPS revice.
        /// </summary>
        public GpsHandler()
        {

        }
	
		
        /// <summary>
        /// Converts GPS position in d"dd.ddd' to decimal degrees ddd.ddddd
        /// </summary>
        /// <param name="dm"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal static double GPSToDecimalDegrees(string dm, string dir)
        {
            try
            {
                if (dm == "" || dir == "")
                {
                    return 0.0;
                }
                //Get the fractional part of minutes
                //DM = '5512.45',  Dir='N'
                //DM = '12311.12', Dir='E'

                double fm = double.Parse(dm.Substring(dm.IndexOf(".")),NumberFormatEnUs);

                //Get the minutes.
                double min = double.Parse(dm.Substring(dm.IndexOf(".") - 2, 2), NumberFormatEnUs);

                //Degrees
                double deg = double.Parse(dm.Substring(0, dm.IndexOf(".") - 2), NumberFormatEnUs);
				
                if (dir == "S" || dir == "W")
                    deg = -(deg + (min + fm) / 60);
                else
                    deg = deg + (min + fm) / 60;
                return deg;
            }
            catch
            {
                return 0.0;
            }
        }
    }
}