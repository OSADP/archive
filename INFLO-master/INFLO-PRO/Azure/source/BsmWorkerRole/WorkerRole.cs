/*!
    @file         BsmWorkerRole/WorkerRole.cs
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
using System.Net;
using System.Threading;
using InfloCommon.Models;
using InfloCommon.InfloDb;

namespace BsmWorkerRole
{
    public class WorkerRole : Microsoft.WindowsAzure.ServiceRuntime.RoleEntryPoint
    {
        /*
         * Static Variables
         */
        private static Microsoft.WindowsAzure.Storage.CloudStorageAccount srStorageAccount;

        private static Microsoft.WindowsAzure.Storage.Queue.CloudQueueClient srCloudQueueClient;
        private static Microsoft.WindowsAzure.Storage.Queue.CloudQueue srBsmQueue;
        private static string srBsmQueueName = "inbound-bsm-bundles";

        private static string srBsmTimeTableName = "BsmTimeTable";

        private static InfloDbContext srDbContext;
        private static string srLogStatisticalDataSettingKey = "Stats_LogStatisticalData";
        private static string srEndToEndLoggingThresholdSettingKey = "Stats_MinimalLoggedElapsedTime";

        /*
         * Member Methods
         */
        public override bool OnStart()
        {
            Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.Changed += RoleEnvironment_Changed;

            Trace.TraceInformation("[TRACE] Entering BsmWorkerRole::OnStart()...");

            ServicePointManager.DefaultConnectionLimit = 12;  //maximum number of concurrent connections 

            string strStorageAccountConnectionString = 
                Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("StorageAccountConnectionString");
            
            if(strStorageAccountConnectionString == null)
            {
                Trace.TraceError("Unable to retrieve storage account connection string");
            }
            else if(strStorageAccountConnectionString.Length <= 0)
            {
                Trace.TraceError("Storage account connection string empty");
            }
            else  //connect to the cloud storage account
            {
                Trace.TraceInformation("[INFO] Retrieved StorageAccountConnectionString for BsmWorkerRole\n{0}", 
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
                    srCloudQueueClient = srStorageAccount.CreateCloudQueueClient();
                    srBsmQueue = srCloudQueueClient.GetQueueReference(srBsmQueueName);

                    try
                    {
                        if (srBsmQueue.CreateIfNotExists())
                        {
                            Trace.TraceInformation("Created Azure BSM queue '{0}'", srBsmQueueName);
                        }
                        else
                        {
                            Trace.TraceInformation("Got reference to existing BSM queue '{0}'", srBsmQueueName);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Exception occurred when creating queue for inbound BSM bundles\n{0}",
                            e.Message);

                        srBsmQueue = null;
                    }

                    BsmTimeTableLogger.Initialize(srStorageAccount, srBsmTimeTableName);
                }
            }

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
                Trace.TraceInformation("[INFO] Retrieved InfloDatabaseConnectionString for BsmWorkerRole\n{0}", 
                    strDatabaseConnectionString);

                srDbContext = new InfloDbContext(strDatabaseConnectionString);
            }

            BsmTimeTableLogger.Enabled = IsStatisticalLoggingEnabled();
            BsmTimeTableLogger.MinimalLoggedElapsedTime = GetMinimalLoggedElapsedTime();

            Trace.TraceInformation("[TRACE] Exiting BsmWorkerRole::OnStart()...");
            return base.OnStart();
        }
        
        public override void Run()
        {
            Trace.TraceInformation("[TRACE] Entering BsmWorkerRole::Run()...");

            if(srBsmQueue == null)
            {
                Trace.TraceError("ERROR: Unable to run BsmWorkerRole-- Inbound BSM queue does not exist");
                Trace.TraceInformation("[TRACE] Exiting BsmWorkerRole::Run()...");
                return;
            }

            const int minPollWaitPeriod_ms = 10;
            const int maxPollWaitPeriod_ms = 1000;
            const int maxQueueMessagePollCount = 32; //32 is the MAX allowed
            const int messageLeaseTime = 30;
            int currPollWaitPeriod_ms = maxPollWaitPeriod_ms;
            
            while(true)
            {
                var rMessages = srBsmQueue.GetMessages(maxQueueMessagePollCount, new TimeSpan(0,0,0,messageLeaseTime * maxQueueMessagePollCount));

                if(rMessages.Count() > 0)
                {
                    List<TME_CVData_Input> rCvDataEntriesToAdd = new List<TME_CVData_Input>();

                    if(currPollWaitPeriod_ms > minPollWaitPeriod_ms)
                    {
                        // Get the queue message count
                        srBsmQueue.FetchAttributes();
                        int? messageCount = srBsmQueue.ApproximateMessageCount;
                
                        Trace.WriteLine(String.Format("Processing {0} new queue messages...",
                            messageCount));
                    }

                    BsmTimeTableLogger.StartNewLogEntry(srBsmQueue, rMessages);

                    foreach (var rMessage in rMessages)
                    {
                        string strMessageJSON = rMessage.AsString;
                        
                        try
                        {
                            /*
                             * Extract raw bsm data from queue message
                             */
                            BsmBundle rBsmBundle = Newtonsoft.Json.JsonConvert.DeserializeObject<BsmBundle>(strMessageJSON);

                            foreach (var bsm in rBsmBundle.payload)
                            {
                                TME_CVData_Input rCvData = null;
                                try
                                {
                                    DateTimeOffset extractStartTime = DateTimeOffset.Now;
                                    //Validate conversion and BSM inside 'GenerateFromBsmMessage'.  Throw exception with errors.
                                    rCvData = GenerateCvDataFromBsmMessage(bsm);

                                    if ((DateTimeOffset.Now - extractStartTime).TotalSeconds > 0.5)
                                        Trace.TraceWarning("Took {0} seconds to extract BSM Data", (DateTimeOffset.Now - extractStartTime).TotalSeconds);
                                }
                                catch (Exception e)
                                {
                                    Trace.TraceWarning("Exception occurred when extracting CV Data from BSM Payload\n{0}",
                                        e.Message);
                                }

                                //If conversion was good, add to DbSet.
                                if (rCvData != null)
                                {
                                    rCvDataEntriesToAdd.Add(rCvData);
                                }
                            }
                        }
                        catch (Newtonsoft.Json.JsonSerializationException e)
                        {
                            Trace.TraceError("Exception when deserializing queue item. Message: '{0}'",
                                e.Message);
                        }
                        finally
                        {
                            currPollWaitPeriod_ms = minPollWaitPeriod_ms;
                        }
                    }

                    BsmTimeTableLogger.SetDeserializationComplete();

                    foreach(var rMessage in rMessages)
                    {
                        try
                        {
                            srBsmQueue.DeleteMessage(rMessage);
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Exception occurred when deleting message from queue.  Did the processing of the BSM take longer then the message's lease time?\n{0}",
                                    e.Message);
                        }
                    }

                    BsmTimeTableLogger.SetQueueDeleteComplete();

                    //Save changes to DB.
                    if (rCvDataEntriesToAdd.Count() > 0)
                    {
                        foreach (var rCvData in rCvDataEntriesToAdd)
                        {
                            try
                            {
                                DateTimeOffset dbSetAddStartTime = DateTimeOffset.Now;
                                /*
                                 * This use to take forever when the dbSet got large.
                                 * This was fixed by adding "base.Configuration.AutoDetectChangesEnabled = false;"
                                 * to the dbContext constructor.
                                 */
                                srDbContext.TME_CVData_Input.Add(rCvData);
                                if ((DateTimeOffset.Now - dbSetAddStartTime).TotalSeconds > 0.5)
                                    Trace.TraceWarning("Took {0} seconds to add CvData to DbSet", (DateTimeOffset.Now - dbSetAddStartTime).TotalSeconds);
                            }
                            catch (Exception e)
                            {
                                Trace.TraceError("Exception occurred when adding CV Data to DbSet\n{0}",
                                    e.Message);
                            }
                        }


                        try
                        {
                            srDbContext.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Exception occurred when saving changes to Db\n{0}", e);
                        }

                        rCvDataEntriesToAdd.ForEach(x => srDbContext.Entry<TME_CVData_Input>(x).State = System.Data.Entity.EntityState.Detached);
                    }

                    BsmTimeTableLogger.SubmitLogEntry(rCvDataEntriesToAdd.Count());
                }
                else //if(rMessages.Count() == 0)
                {
                    Trace.WriteLine(String.Format("Waiting {0}ms for new {1} queue items...", 
                        currPollWaitPeriod_ms, srBsmQueueName));

                    Thread.Sleep(currPollWaitPeriod_ms);

                    // Gradually scale up the poll wait period until we reach the max (10,20,40,80,160,...1000)
                    currPollWaitPeriod_ms *= 2;
                    currPollWaitPeriod_ms = Math.Min(currPollWaitPeriod_ms, maxPollWaitPeriod_ms);
                }
            }
        }

        void RoleEnvironment_Changed(object sender, Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentChangedEventArgs e)
        {
            if (e.Changes.OfType<Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentConfigurationSettingChange>().Count() > 0)
            {
                BsmTimeTableLogger.Enabled = IsStatisticalLoggingEnabled();
                BsmTimeTableLogger.MinimalLoggedElapsedTime = GetMinimalLoggedElapsedTime();
            }
        }


        /*
         * Static Methods
         */
        public static TME_CVData_Input GenerateCvDataFromBsmMessage(BsmMessage bsm)
        {
            TME_CVData_Input results = new TME_CVData_Input();
            
            byte[] rawBsm;

            try
            {
                //Converted payload to raw bsm byte[] (ASN.1)
                rawBsm = Enumerable.Range(0, bsm.payload.Length)
                    .Where(i => i % 2 == 0)
                    .Select(i => Convert.ToByte(bsm.payload.Substring(i, 2), 16))
                    .ToArray();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to parse bsm.payload to byte[]: {0}", bsm.payload), e);
            }

            ExtractedBSM rExtractedBsm = new ExtractedBSM();

            //Try to extract the BSM from the raw ASN.1 byte[]
            if (!rExtractedBsm.loadFromASN(rawBsm))
            {
                throw new Exception(string.Format("Error deserializing BSM from ASN.1: {0}",
                    string.Concat(Array.ConvertAll(rawBsm, b => b.ToString("X2")))));
            }
            
            results.NomadicDeviceID = rExtractedBsm.getNomadicId();
            results.DateGenerated = DateTime.Parse(bsm.time);
            results.Speed = (short)rExtractedBsm.getSpeed() * 2.236936292054402;
            results.Heading = (short)rExtractedBsm.getHeading();
            results.Latitude = rExtractedBsm.getLatitude();
            results.Longitude = rExtractedBsm.getLongitude();
            results.MMLocation = rExtractedBsm.getMileMarker();
            results.CVQueuedState = rExtractedBsm.getQueuedState();
            results.CoefficientOfFriction = rExtractedBsm.getCoefOfFriction();
            results.Temperature = (short)rExtractedBsm.getAirTemp();
            results.RoadwayId = rExtractedBsm.getRoadwayId();
            results.LateralAcceleration = rExtractedBsm.getLatAccel();
            results.LongitudinalAcceleration = rExtractedBsm.getLongAccel();

            return results;
        }

        public static bool IsStatisticalLoggingEnabled()
        {
            bool results = false;

            string enabledString = Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting(srLogStatisticalDataSettingKey);
            if (enabledString != null)
            {
                try
                {
                    results = Convert.ToBoolean(enabledString);
                }
                catch (FormatException) { Trace.TraceWarning("Unable to parse setting key value to boolean: {0}-{1}", srLogStatisticalDataSettingKey, enabledString); }
            }

            return results;
        }

        public static int GetMinimalLoggedElapsedTime()
        {
            int results = 0;

            string enabledString = Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting(srEndToEndLoggingThresholdSettingKey);
            if (enabledString != null)
            {
                try
                {
                    results = Convert.ToInt32(enabledString);
                }
                catch (FormatException) { Trace.TraceWarning("Unable to parse setting key value to int: {0}-{1}", srLogStatisticalDataSettingKey, enabledString); }
            }

            return results;
        }
    }
}
