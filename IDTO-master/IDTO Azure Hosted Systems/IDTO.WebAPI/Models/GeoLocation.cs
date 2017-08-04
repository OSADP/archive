using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace IDTO.WebAPI.Models
{
    /// <summary>
    /// Takes address as input(any string) and returns Google's interpretation
    /// </summary>

    public class GeoLocation
    {
        public GeoLocation()
        { }

        public GeolookupResult GetLatLongForAddress(string location)
        {
            //Initialize     
            var retVal = new GeolookupResult();
            retVal.position = new float[2];
            retVal.position[0] = 0;
            retVal.position[1] = 0;
            retVal.APIReturnStatus = string.Empty;

            try
            {
                string result = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=true&key=AIzaSyAB90Da_V_KehN1etaqmwaQ7vgJv6fwhXI", location);

                retVal.position = new float[2];
                retVal.position[0] = 0;
                retVal.position[1] = 0;

                var doc = XDocument.Load(result);

                string status = doc.Element("GeocodeResponse").Element("status").Value;

                retVal.APIReturnStatus = status.ToString();

                if (status.Equals("OK"))
                {
                    var point = doc.Element("GeocodeResponse").Element("result").Element("geometry").Element("location");
                    retVal.FormattedAddress = doc.Element("GeocodeResponse").Element("result").Element("formatted_address").Value;
                    string lat = point.Element("lat").Value;
                    string lng = point.Element("lng").Value;

                    retVal.position[0] = (float)Convert.ToDouble(lat);
                    retVal.position[1] = (float)Convert.ToDouble(lng);
                }

                return retVal;
            }
            catch (Exception e)
            {
                retVal.APIReturnStatus = e.Message;
                return retVal;
            }
        }

        public class GeolookupResult
        {
            /// <summary>
            /// Stores lat[0] and lng[1]
            /// </summary>
            public float[] position { get; set; }
            public string FormattedAddress { get; set; }
            /// <summary>
            /// Return text from api call
            /// </summary>
            public string APIReturnStatus { get; set; }
        }
    }
}