using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using INCZONE.Common;
using log4net;

namespace INCZONE.Managers
{
    public class CapWINManager
    {
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        private string _HostUrl;
        private string _Username;
        private string _Password;

        /// <summary>
        /// 
        /// </summary>
        public CapWINManager() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CapWINConfig"></param>
        public CapWINManager(CapWINConfig CapWINConfig) 
        {
            this._HostUrl = CapWINConfig.HostURL;
            this._Password = CapWINConfig.Password;
            this._Username = CapWINConfig.Username;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="HostUrl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>XmlDocument</returns>
        public static XmlDocument TestCapWINSettings(string HostUrl, string username, string password)
        {
 //           log.Debug("In TestCapWINSettings");

            XmlDocument CapWINXML = null;

            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
                using (WebClient wc = new WebClient())
                {
                    wc.Credentials = new NetworkCredential(username, password);

                    string xml = wc.DownloadString(HostUrl);

                    CapWINXML = new XmlDocument();
                    CapWINXML.LoadXml(xml);
                }
            }
            catch (NotSupportedException e)
            {
                log.Error("NotSupportedException", e);
                throw e;
            }
            catch (ArgumentNullException e)
            {
                log.Error("ArgumentNullException", e);
                throw e;
            }
            catch (WebException e)
            {
                log.Error("WebException", e);
                throw e;
            }
            catch (XmlException e)
            {
                log.Error("XmlException", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception", e);
                throw e;
            }

            return CapWINXML;
        }

        public async Task<CapWINIncidentListType1> GetCapWINIncidentsList()
        {
//            log.Debug("In CapWINIncidents");
            XmlDocument CapWINXML = null;
            string xml;

            try
            {

                using (var handler = new HttpClientHandler
                    {
                        Credentials = new
                            NetworkCredential(this._Username, this._Password)
                    })
                {
                    using (var client = new HttpClient(handler))
                    {
                        client.MaxResponseContentBufferSize = 2147483647;
                        xml = await client.GetStringAsync(this._HostUrl);
                    }
                }

                CapWINXML = new XmlDocument();
                CapWINXML.LoadXml(xml);

            }
            catch (NotSupportedException e)
            {
                log.Error("NotSupportedException", e);
                throw e;
            }
            catch (ArgumentNullException e)
            {
                log.Error("ArgumentNullException", e);
                throw e;
            }
            catch (WebException e)
            {
                log.Error("WebException", e);
                throw e;
            }
            catch (XmlException e)
            {
                log.Error("XmlException", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception", e);
                throw e;
            }

            return MessageManager.CapWINXMLDigester(CapWINXML);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CapWINIncidentListType"></param>
        /// <param name="_ResponderLocation"></param>
        /// <param name="DistanceToIncident"></param>
        /// <returns></returns>
        internal static Segment GetCapWINIncidents(CapWINIncidentListType1 CapWINIncidentListType, Coordinate _ResponderLocation, int DistanceToIncident)
        {
            //log.Debug("In GetCapWINIncidents");

            Segment Segment = null;

            try
            {
                if (CapWINIncidentListType != null)
                {
                    if (CapWINIncidentListType.CapWINIncident != null)
                    {
                        //log.Debug("In CapWIN Incident: " + CapWINIncidentListType.CapWINIncident.Length);
                        foreach (CapWINIncidentType CapWINIncident in CapWINIncidentListType.CapWINIncident)
                        {
                            if (CapWINIncident != null && CapWINIncident.IncidentLink != null)
                            {
                                foreach (LocationType Location in CapWINIncident.IncidentLocation)
                                {
                                    if (Location != null)
                                    {
                                        if (Location.LocationTwoDimensionalGeographicCoordinate != null)
                                        {
                                            decimal latitude = Location.LocationTwoDimensionalGeographicCoordinate[0].GeographicCoordinateLatitude[0].LatitudeDegreeValue[0].Value;
                                            decimal longitude = Location.LocationTwoDimensionalGeographicCoordinate[0].GeographicCoordinateLongitude[0].LongitudeDegreeValue[0].Value;
                                            double distance = _ResponderLocation.Distance(new Coordinate((double)longitude, (double)latitude));

                                            //log.Debug("Longitude: " + longitude + " Latitude: " + latitude + " Distance: " + distance);
                                            if (distance <= DistanceToIncident)
                                            {
                                                Segment = new Segment();
                                                Segment.SegmentType = CapWINIncident.IncidentLink.LinkComponents.Segment;
                                                Segment.TimeofIncident = CapWINIncident.CreationDate;
                                                Segment.IncidentName = CapWINIncident.ActivityName[0].Value;
                                                String name = CapWINIncident.ActivityName[0].Value;
                                                log.Debug("Found Incident: " + name);

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }

            return Segment;
        }

        public static List<CapWINLocation> GetCapWINIncidentsWithLocations(CapWINIncidentListType1 CapWINIncidentListType)
        {
            //log.Debug("In GetCapWINIncidentsWithLocations");

            List<CapWINLocation> returnList = new List<CapWINLocation>();

            try
            {
                if (CapWINIncidentListType != null)
                {
                    if (CapWINIncidentListType.CapWINIncident != null)
                    {
                        //log.Debug("In CapWIN Incident: " + CapWINIncidentListType.CapWINIncident.Length);
                        foreach (CapWINIncidentType CapWINIncident in CapWINIncidentListType.CapWINIncident)
                        {
                            string DisplayId = CapWINIncident.ActivityName[0].Value;
                            if (CapWINIncident != null && CapWINIncident.IncidentLink != null)
                            {
                                foreach (LocationType Location in CapWINIncident.IncidentLocation)
                                {
                                    if (Location != null)
                                    {
                                        if (Location.LocationTwoDimensionalGeographicCoordinate != null)
                                        {
                                            returnList.Add(new CapWINLocation(DisplayId, Location));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }

            return returnList;
        }

    }
}
