/*!
    @file         InfloWebRole/Controllers/I2VController.cs
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

namespace InfloWebRole.Controllers
{
    public class MapController : ApiController
    {

        public static bool Between(double num, double lower, double upper, bool inclusive = false)
        {
            if (lower > upper)
            {
                double temp = lower;
                lower = upper;
                upper = temp;
            }

            return inclusive
                ? lower <= num && num <= upper
                : lower < num && num < upper;
        }

        private const int TimAgencyId = 37;
        private static double AlertSearchDistance = 2.0;

        private static InfloDbContext srInfloDbContext;

        static MapController()
        {
            Trace.TraceInformation("[TRACE] Entering MapController::MapController() static initializer...");

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


            Trace.TraceInformation("[TRACE] Exiting MapController::MapController() static initializer...");
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

        // GET api/map
        public IEnumerable<string> Get()
        {
            string strWelcomeMessage =
                String.Format("InfloWebRole::MapController - Last Updated {0} UTC", this.GetLastBuildTimestamp_UTC());

            if (srInfloDbContext == null)
            {
                return new string[] { string.Concat(strWelcomeMessage, "\n", "Database Connection: Not Connected") };
            }
            else
            {
                var roadways = srInfloDbContext.Configuration_Roadway.Select(r => r.Name + "='" + r.RoadwayId + "'").ToList();
                roadways.Insert(0, strWelcomeMessage);
                return roadways.ToArray();
            }
        }

        // GET api/map?roadwayid=xxx
        public HttpResponseMessage Get(string roadwayid, int startIndex = 0, int linkCount = 16)
        {
            Trace.TraceInformation("[TRACE] Entering MapController::Get(string roadwayid)...");
            
            if (srInfloDbContext == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error connecting to Inflo DB");
            }
            else
            {
                var results = generateMapMessage(roadwayid, startIndex, linkCount);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(results));
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = String.Format("inflo_map-{0}_{1}_{2}.ber", roadwayid, startIndex, linkCount);

                return response;
            }

        }

        private byte[] generateMapMessage(string roadwayid, int startIndex, int linkCount)
        {
            if (srInfloDbContext == null)
                return new byte[0];

            Configuration_Roadway roadway = srInfloDbContext.Configuration_Roadway.Where(r => r.RoadwayId.Equals(roadwayid)).FirstOrDefault();
            if (roadway == null)
                return new byte[0];

            IEnumerable<Configuration_RoadwayMileMarkers> dbMileMarkers = srInfloDbContext.Configuration_RoadwayMileMarkers
                .Where(r => r.RoadwayId.Equals(roadway.RoadwayId));

            var dbLinks = srInfloDbContext.Configuration_RoadwayLinks
                .Where(l => l.RoadwayId.Equals(roadway.RoadwayId)).ToList()
                .Where(l => Between(l.BeginMM, roadway.BeginMM, roadway.EndMM, true) && Between(l.EndMM, roadway.BeginMM, roadway.EndMM, true))
                .ToArray();
                //.Where(l => roadway.BeginMM <= l.BeginMM && l.EndMM <= roadway.EndMM).OrderBy(l => l.BeginMM);

            List<ExtractedMapIntersection> asnIntersections = new List<ExtractedMapIntersection>();

            for (int linkIndex = startIndex; (linkIndex < dbLinks.Length) && (linkIndex < startIndex + linkCount); linkIndex++)
            {
                var link = dbLinks[linkIndex];

                List<Extracted3DOffset> increasingNodes = new List<Extracted3DOffset>();
                List<Extracted3DOffset> decreasingNodes = new List<Extracted3DOffset>();

                var dbLinkMileMarkers = dbMileMarkers.ToList()
                    .Where(m => Between(m.MMNumber, link.BeginMM, link.EndMM, true))
                    //.Where(m => link.BeginMM <= m.MMNumber && m.MMNumber <= link.EndMM)
                    .OrderBy(m => m.MMNumber).ToArray();

                for (int i = 0; i < dbLinkMileMarkers.Length; i++)
                {
                    var mileMarker = dbLinkMileMarkers[i];
                    increasingNodes.Insert(i, new Extracted3DOffset(mileMarker.Latitude1, mileMarker.Longitude1, mileMarker.MMNumber));
                    decreasingNodes.Insert(0, new Extracted3DOffset(mileMarker.Latitude2, mileMarker.Longitude2, mileMarker.MMNumber));
                }
                ExtractedNodeList increasingApproach = new ExtractedNodeList(increasingNodes.ToArray());
                increasingApproach.setName(link.LinkId + "_inc");
                ExtractedNodeList decreasingApproach = new ExtractedNodeList(decreasingNodes.ToArray());
                decreasingApproach.setName(link.LinkId + "_dec");

                asnIntersections.Add(new ExtractedMapIntersection(link.LinkId, new ExtractedNodeList[] { increasingApproach, decreasingApproach }));
            }

            ExtractedMap results = new ExtractedMap(roadway.RoadwayId + "\n" + roadway.Name);
            results.setIntersections(asnIntersections.ToArray());

            return results.generateASN();
        }


    }
}
