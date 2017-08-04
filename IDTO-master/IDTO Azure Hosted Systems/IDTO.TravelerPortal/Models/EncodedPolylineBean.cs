using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.TravelerPortal.Models
{
    public class EncodedPolylineBean
    {
        public string points { get; set; }
        public string levels { get; set; }
        public int length { get; set; }
    }

    public static class GooglePoints
    {
        public static List<CoordinateEntity> Decode(string encodedPoints)
        {
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            List<CoordinateEntity> list = new List<CoordinateEntity>();

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                list.Add(new CoordinateEntity
                { 
                    Latitude = Convert.ToDouble(currentLat) / 1E5, 
                    Longitude = Convert.ToDouble(currentLng) / 1E5  
                });
            }

            return list;
        }
    }

    public class CoordinateEntity
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
