/*!
    @file         I2VWebAPI/Controllers/I2VController.cs
    @author       Luke Kucalaba, Joshua Branch

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using InfloCommon.Models;
using InfloCommon.InfloDb;

namespace I2VWebAPI.Controllers
{
    public class TimController : ApiController
    {

        private const int TimAgencyId = 37;
        private static double AlertSearchDistance = 2.0;

        private static InfloDbContext srInfloDbContext;
        
        static TimController()
        {
            Trace.TraceInformation("[TRACE] Entering TimController::TimController() static initializer...");

            string strDatabaseConnectionString =
                Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("InfloDatabaseConnectionString");

            if (strDatabaseConnectionString == null)
            {
                Trace.TraceError("Unable to retrieve database connection string");
            }
            else if (strDatabaseConnectionString.Length <= 0)
            {
                Trace.TraceError("Database connection string empty");
            }
            else  //connect to the database
            {
                srInfloDbContext = new InfloDbContext(strDatabaseConnectionString);
            }


            Trace.TraceInformation("[TRACE] Exiting TimController::TimController() static initializer...");
            return;
        }

        private string GetLastBuildTimestamp_UTC()
        {
            try
            {
                string strCallingAssemblyFilename = Assembly.GetCallingAssembly().Location;
                return File.GetLastWriteTime(strCallingAssemblyFilename).ToUniversalTime().ToString();
            }
            catch(Exception e)
            {
                Trace.TraceError("Exception occurred in GetLastBuildTimestamp_UTC()\n{0}",
                    e.Message);

                return "<UNKNOWN>";
            }
        }

        // GET api/tim
        public IEnumerable<string> Get()
        {
            string strWelcomeMessage =
                String.Format("I2VWebAPI::TimController - Last Updated {0} UTC", this.GetLastBuildTimestamp_UTC());

            if (srInfloDbContext == null)
            {
                return new string[] { string.Concat(strWelcomeMessage, "\n", "Database Connection: Not Connected") };
            }
            else
            {
                return new string[] { string.Concat(strWelcomeMessage, "\n", "Database Connection: Connected!") };
            }
        }

        // GET api/tim?linkid=xxx
        public HttpResponseMessage Get(string linkid)
        {
            Trace.TraceInformation("[TRACE] Entering TimController::Get(string linkid)...");
            
            if (srInfloDbContext == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error connecting to Inflo DB");
            }
            else
            {
                TMEOutput_Recommended_QWARN_CVMessage[] rQWarnAlerts;
                LoadApplicableAlerts(linkid, out rQWarnAlerts);

                try{
                    var rawTimMessageContent = GenerateTimFromAlerts(rQWarnAlerts);//new byte[] { 0x30, 0x0F, 0x80, 0x01, 0x10, 0x81, 0x04, 0x25, 0x0f, 0x35, 0xad, 0xa4, 0x00, 0x85, 0x02, 0x00, 0x00 };
                    
                    TimMessage rTimMessage = new TimMessage();
                    rTimMessage.payload = string.Concat(Array.ConvertAll(rawTimMessageContent, b => b.ToString("X2")));

                    return this.Request.CreateResponse<TimMessage>(HttpStatusCode.OK, rTimMessage);
                }
                catch(Exception)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error generating TIM Message");
                }
            }
        }



        private static bool LoadApplicableAlerts(string linkId, out TMEOutput_Recommended_QWARN_CVMessage[] qWarnAlerts)
        {
            Configuration_RoadwayLinkInformation currentLink = srInfloDbContext.Configuration_RoadwayLinkInformation
                    .Find(linkId);

            if (false && currentLink != null)
            {
                IEnumerable<string> downstreamLinks = GetDownstreamLinks(currentLink, AlertSearchDistance);

                //Select the latest alert for each roadwayLink.
                qWarnAlerts = downstreamLinks.Select(x =>
                    srInfloDbContext.TMEOutput_Recommended_QWARN_CVMessage
                    .Where(q => q.RoadwayLinkID.Equals(x))
                    .OrderByDescending(q => q.Timestamp)
                    .FirstOrDefault())
                    .Where(q => q != null).ToArray();

                return true;
            }

            qWarnAlerts = new TMEOutput_Recommended_QWARN_CVMessage[0];

            return false;
        }

        private static string[] GetDownstreamLinks(Configuration_RoadwayLinkInformation linkId, double maxDistance)
        {
            List<string> results = new List<string>();
            results.Add(linkId.RoadwayLinkIdentifier);

            if (linkId.DownstreamRoadwayLinkIdentifier != null)
            {
                Configuration_RoadwayLinkInformation nextLink = srInfloDbContext.Configuration_RoadwayLinkInformation
                    .Find(linkId.DownstreamRoadwayLinkIdentifier);

                if (nextLink != null)
                {
                    maxDistance -= Math.Abs(linkId.EndMileMarker - nextLink.EndMileMarker);

                    if (maxDistance > 0)
                        results.AddRange(GetDownstreamLinks(nextLink, maxDistance));
                }
            }

            return results.ToArray();
        }

        private static byte[] GenerateTimFromAlerts(TMEOutput_Recommended_QWARN_CVMessage[] qWarnAlerts)
        {
            ExtractedTIM timMessage = new ExtractedTIM();
            timMessage.setPacketId(TimAgencyId, DateTime.Now);

            return timMessage.generateASN();
        }
    }
}
