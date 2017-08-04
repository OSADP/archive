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
using System.Data.Entity;
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
    public class TimController : ApiController
    {
        private const int TimAgencyId = 37;
        private const int AlertDefaultDuration = 10;
        private const int MileMarkersPerValidRegion = 15;
        private static double AlertSearchRadius;
        private static double QWarnAheadDistance;

        private static InfloDbContext srInfloDbContext;

        private static Microsoft.WindowsAzure.Storage.CloudStorageAccount srStorageAccount;
        private static Microsoft.WindowsAzure.Storage.Table.CloudTableClient srCloudTableClient;
        private static Microsoft.WindowsAzure.Storage.Table.CloudTable srTimV2VTable;
        private static string srTimV2VTableName = "TimV2VTable";

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

        static TimController()
        {
            Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.Changed += RoleEnvironment_Changed;

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

            string strStorageAccountConnectionString =
                Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("StorageAccountConnectionString");

            if (strStorageAccountConnectionString == null)
            {
                Trace.TraceError("Unable to retrieve storage account connection string");
            }
            else if (strStorageAccountConnectionString.Length <= 0)
            {
                Trace.TraceError("Storage account connection string empty");
            }
            else  //connect to the cloud storage account
            {
                Trace.TraceInformation("[INFO] Retrieved StorageAccountConnectionString for TimController\n{0}",
                    strStorageAccountConnectionString);

                try
                {
                    srStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(strStorageAccountConnectionString);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception occurred when parsing storage account connection string\n{0}\n{1}",
                        strStorageAccountConnectionString, e.Message);
                }

                if (srStorageAccount != null)
                {
                    srCloudTableClient = srStorageAccount.CreateCloudTableClient();
                    srTimV2VTable = srCloudTableClient.GetTableReference(srTimV2VTableName);

                    try
                    {
                        srTimV2VTable.CreateIfNotExists();
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Exception occurred when creating table for V2V TIMs\n{0}",
                            e.Message);

                        srTimV2VTable = null;
                    }
                }
            }

            AlertSearchRadius = GetAlertSearchDistance();
            QWarnAheadDistance = GetQWarnAheadDistance();

            srInfloDbContext.Configuration_Roadway.Load();
            srInfloDbContext.Configuration_RoadwayLinks.Load();
            srInfloDbContext.Configuration_RoadwayMileMarkers.Load();
            srInfloDbContext.TMEOutput_QWARNMessage_CV.Load();
            srInfloDbContext.TMEOutput_SPDHARMMessage_CV.Load();

            Trace.TraceInformation("[TRACE] Exiting TimController::TimController() static initializer...");
            return;
        }

        static void RoleEnvironment_Changed(object sender, Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentChangedEventArgs e)
        {
            if (e.Changes.OfType<Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentConfigurationSettingChange>().Count() > 0)
            {
                AlertSearchRadius = GetAlertSearchDistance();
                QWarnAheadDistance = GetQWarnAheadDistance();
            }
        }

        public static double GetAlertSearchDistance()
        {
            double results = 1.0;

            string value = Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("AlertSearchDistance");
            if (value != null)
            {
                try
                {
                    results = Convert.ToDouble(value);
                }
                catch (FormatException) { Trace.TraceWarning("Unable to parse setting key value to double: {0}-{1}", "AlertSearchDistance", value); }
            }

            return results;
        }

        public static double GetQWarnAheadDistance()
        {
            double results = 1.0;

            string value = Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("QWarnAheadDistance");
            if (value != null)
            {
                try
                {
                    results = Convert.ToDouble(value);
                }
                catch (FormatException) { Trace.TraceWarning("Unable to parse setting key value to double: {0}-{1}", "QWarnAheadDistance", value); }
            }

            return results;
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

        public HttpResponseMessage Post([FromBody]TimMessage message)
        {
            if (this.ModelState.IsValid)
            {
                if (srTimV2VTable != null)
                {
                    srTimV2VTable.ExecuteAsync(
                        Microsoft.WindowsAzure.Storage.Table.TableOperation.Insert(
                        new TimV2VTableEntity(message)));
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // GET api/tim
        public HttpResponseMessage Get(string roadwayid, double mm)
        {
            Trace.TraceInformation("[TRACE] Entering TimController::Get(string roadwayid, double mm)...");
            
            if (srInfloDbContext == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error connecting to Inflo DB");
            }
            else
            {
                srInfloDbContext.ChangeTracker.DetectChanges();
                
                //TODO reduce alerts we care about.

                var qWarnRoadwayAlerts = srInfloDbContext.TMEOutput_QWARNMessage_CV.Where(a => a.RoadwayID.Equals(roadwayid))/*.Where(a => System.Data.Entity.DbFunctions.AddSeconds(a.DateGenerated, a.ValidityDuration ?? 20) >= DateTime.UtcNow)*/;
                var qWarnIncrAlerts = qWarnRoadwayAlerts.Where(a => a.BOQMMLocation <= a.FOQMMLocation).Where(a => (mm + AlertSearchRadius) >= a.BOQMMLocation && (mm - AlertSearchRadius) <= a.FOQMMLocation).OrderByDescending(a => a.DateGenerated).Take(1);
                var qWarnDecrAlerts = qWarnRoadwayAlerts.Where(a => a.BOQMMLocation > a.FOQMMLocation).Where(a => (mm - AlertSearchRadius) <= a.BOQMMLocation && (mm + AlertSearchRadius) >= a.FOQMMLocation).OrderByDescending(a => a.DateGenerated).Take(1);
                var qWarnApplicableAlerts = qWarnIncrAlerts.Union(qWarnDecrAlerts).ToList();
                
                var qWarnAheadIncrAlerts = qWarnRoadwayAlerts.Where(a => a.BOQMMLocation <= a.FOQMMLocation).Where(a => (mm + QWarnAheadDistance + AlertSearchRadius) >= a.BOQMMLocation && (mm - AlertSearchRadius) <= a.BOQMMLocation).OrderByDescending(a => a.DateGenerated).Take(1);
                var qWarnAheadDecrAlerts = qWarnRoadwayAlerts.Where(a => a.BOQMMLocation > a.FOQMMLocation).Where(a => (mm - QWarnAheadDistance - AlertSearchRadius) <= a.BOQMMLocation && (mm + AlertSearchRadius) >= a.BOQMMLocation).OrderByDescending(a => a.DateGenerated).Take(1);
                var qWarnAheadApplicableAlerts = qWarnAheadIncrAlerts.Union(qWarnAheadDecrAlerts).ToList();

                var spdHarmRoadwayAlerts = srInfloDbContext.TMEOutput_SPDHARMMessage_CV.Where(a => a.RoadwayId.Equals(roadwayid)).Where(a => System.Data.Entity.DbFunctions.AddSeconds(a.DateGenerated, a.ValidityDuration ?? 20) >= DateTime.UtcNow);
                var spdHarmIncrAlerts = spdHarmRoadwayAlerts.Where(a => a.BeginMM <= a.EndMM).Where(a => (mm + AlertSearchRadius) >= a.BeginMM && (mm - AlertSearchRadius) <= a.EndMM).OrderByDescending(a => a.DateGenerated).Take(1);
                var spdHarmDecrAlerts = spdHarmRoadwayAlerts.Where(a => a.BeginMM > a.EndMM).Where(a => (mm - AlertSearchRadius) <= a.BeginMM && (mm + AlertSearchRadius) >= a.EndMM).OrderByDescending(a => a.DateGenerated).Take(1);
                var spdHarmApplicableAlerts = spdHarmIncrAlerts.Union(spdHarmDecrAlerts).ToList();

                try
                {
                    ExtractedTIM timMessage = new ExtractedTIM();
                    timMessage.setPacketId(TimAgencyId, DateTime.Now);

                    List<ExtractedTimFrame> frames = new List<ExtractedTimFrame>();

                    foreach (var alert in qWarnApplicableAlerts)
                    {
                        double startMM = alert.BOQMMLocation;
                        double endMM = (double)alert.FOQMMLocation;

                        double length = Math.Abs(endMM - startMM);
                        int time = -1;
                        if (alert.SpeedInQueue != null && alert.SpeedInQueue != 0)
                            time = (int)(length * 60 / (short)alert.SpeedInQueue);
                        int duration = alert.ValidityDuration == null ? AlertDefaultDuration : (int)alert.ValidityDuration;

                        ExtractedTimFrame frame = BuildAlertFrame(roadwayid, String.Format("q,{0:F1},{1},{2}", length, time, "Q-WARN: In Queue"), alert.DateGenerated, duration, startMM, endMM);

                        if (frame.getAlertPathCount() > 0)
                        {
                            frames.Add(frame);
                        }
                    }

                    foreach (var alert in qWarnAheadApplicableAlerts)
                    {
                        double endMM = alert.BOQMMLocation;
                        double startMM;
                        if (alert.BOQMMLocation > alert.FOQMMLocation)
                            startMM = endMM + QWarnAheadDistance;
                        else
                            startMM = endMM - QWarnAheadDistance;

                        double length = Math.Abs(endMM - startMM);
                        int time = -1;
                        if (alert.SpeedInQueue != null && alert.SpeedInQueue != 0)
                            time = (int)(length * 60 / (short)alert.SpeedInQueue);
                        int duration = alert.ValidityDuration == null ? AlertDefaultDuration : (int)alert.ValidityDuration;

                        ExtractedTimFrame frame = BuildAlertFrame(roadwayid, String.Format("a,{0:F1},{1},{2}", length, time, "Q-WARN: Queue Ahead"), alert.DateGenerated, duration, startMM, endMM);

                        if (frame.getAlertPathCount() > 0)
                        {
                            frames.Add(frame);
                        }
                    }

                    foreach (var alert in spdHarmApplicableAlerts)
                    {
                        double startMM = alert.BeginMM;
                        double endMM = (double)alert.EndMM;

                        int duration = alert.ValidityDuration == null ? AlertDefaultDuration : (int)alert.ValidityDuration;
                        ExtractedTimFrame frame = BuildAlertFrame(roadwayid, String.Format("s,{0},{1}", alert.RecommendedSpeed, alert.Justification), alert.DateGenerated, duration, startMM, endMM);

                        if (frame.getAlertPathCount() > 0)
                        {
                            frames.Add(frame);
                        }
                    }

                    timMessage.setFrames(frames.ToArray());

                    var rawTimMessageContent = timMessage.generateASN();

                    TimMessage rTimMessage = new TimMessage();
                    rTimMessage.payload = string.Concat(Array.ConvertAll(rawTimMessageContent, b => b.ToString("X2")));

                    return this.Request.CreateResponse<TimMessage>(HttpStatusCode.OK, rTimMessage);
                }
                catch(Exception e)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error generating TIM Message", e);
                }
            }
        }

        ExtractedTimFrame BuildAlertFrame(string roadwayid, string text, DateTime startTime, int duration, double startMM, double endMM)
        {
            bool reversed = startMM > endMM;
            if (reversed)
            {
                double swap = startMM;
                startMM = endMM;
                endMM = swap;
            }

            ExtractedTimFrame results = new ExtractedTimFrame(text, startTime, duration);

            var dbAlertMileMarkers = srInfloDbContext.Configuration_RoadwayMileMarkers
                .Where(r => r.RoadwayId.Equals(roadwayid));

            //Snap to closest milemarkers by widening the alert.
            try
            {
                startMM = dbAlertMileMarkers.Where(m => m.MMNumber <= startMM).OrderByDescending(m => m.MMNumber).First().MMNumber;
            }
            catch { }
            try
            { 
                endMM = dbAlertMileMarkers.Where(m => m.MMNumber >= endMM).OrderBy(m => m.MMNumber).First().MMNumber; 
            }
            catch { }

            dbAlertMileMarkers = dbAlertMileMarkers.Where(m => startMM <= m.MMNumber && m.MMNumber <= endMM);

            if (reversed)
                dbAlertMileMarkers = dbAlertMileMarkers.OrderByDescending(m => m.MMNumber);
            else
                dbAlertMileMarkers = dbAlertMileMarkers.OrderBy(m => m.MMNumber);

            var dbAlertMileMarkersList = dbAlertMileMarkers.ToList();

            List<ExtractedNodeList> alertPaths = new List<ExtractedNodeList>();
            while(dbAlertMileMarkersList.Count() > 1)
            {
                var mmarkers = dbAlertMileMarkersList.GetRange(0, Math.Min(MileMarkersPerValidRegion, dbAlertMileMarkersList.Count()));
                List<Extracted3DOffset> nodes = new List<Extracted3DOffset>();
                
                if (reversed)
                    mmarkers.OrderByDescending(m => m.MMNumber).ToList().ForEach(m => nodes.Add(new Extracted3DOffset(m.Latitude2, m.Longitude2, m.MMNumber)));
                else
                    mmarkers.OrderBy(m => m.MMNumber).ToList().ForEach(m => nodes.Add(new Extracted3DOffset(m.Latitude1, m.Longitude1, m.MMNumber)));
                
                if (nodes.Count > 1)
                {
                    ExtractedNodeList alertPath = new ExtractedNodeList(nodes.ToArray());
                    alertPaths.Add(alertPath);
                }

                dbAlertMileMarkersList.RemoveRange(0, mmarkers.Count() - 1);
            }

            results.setAlertPaths(alertPaths.ToArray());

            return results;
        }
    }
}
