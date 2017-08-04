using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDTO.DataProcessor.TConnectMonitor
{
    class Conversions
    {

        public static double convertMetersToMiles(double meters)
        {
            double miles = meters / 1609.344;
            return miles;
        }

        public static double convertMilesToMeters(double miles)
        {
            double meters = miles * 1609.344;
            return meters;
        }

        public static double convertMetersPerSecToMilesPerHour(double mps)
        {
            double mph = mps * 2.2369362920544025;
            return mph;
        }

        public static double convertDegreesToRadians(double degrees)
        {
            double radians = degrees * (Math.PI / 180.0);
            return radians;
        }

        public static double convertRadiansToDegrees(double radians)
        {
            double degrees = radians * (180.0 / Math.PI);
            return degrees;
        }

        public static int convertMetersPerSecToMPH(double mps)
        {
            double mph = convertMetersPerSecToMilesPerHour(mps);

            double mph_int = Math.Round((double)mph);

            return (int)mph_int;
        }

        public static double distanceMeters(double degreesLat1, double degreesLon1,
                double degreesLat2, double degreesLon2)
        {
            // find radius
            double earthRadius = 6371;
            double dLat = Conversions.convertDegreesToRadians(degreesLat1)
                    - Conversions.convertDegreesToRadians(degreesLat2);
            double dLon = Conversions.convertDegreesToRadians(degreesLon1)
                    - Conversions.convertDegreesToRadians(degreesLon2);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(Conversions.convertDegreesToRadians(degreesLat2))
                    * Math.Cos(Conversions.convertDegreesToRadians(degreesLat1))
                    * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance_km = earthRadius * c;
            double distance_m = distance_km * 1000.0;
            return distance_m;
        }

    }
}
