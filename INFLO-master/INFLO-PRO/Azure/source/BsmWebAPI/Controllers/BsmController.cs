/*!
    @file         BsmWebAPI/Controllers/BsmController.cs
    @author       Luke Kucalaba

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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using InfloCommon.Models;

namespace BsmWebAPI.Controllers
{
    public class BsmController : ApiController
    {
        private static Microsoft.WindowsAzure.Storage.CloudStorageAccount srStorageAccount;
        private static Microsoft.WindowsAzure.Storage.Queue.CloudQueueClient srCloudQueueClient;
        private static Microsoft.WindowsAzure.Storage.Queue.CloudQueue srBsmQueue;
        private static string srBsmQueueName = "inbound-bsm-bundles";
        
        static BsmController()
        {
            Trace.TraceInformation("[TRACE] Entering BsmController::BsmController() static initializer...");

            // NOTE: Need to fully qualify System.Configuration to disambiguate from ApiController.Configuration
            string strStorageAccountConnectionString =
                System.Configuration.ConfigurationManager.AppSettings["StorageAccountConnectionString"];

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
                try
                {
                    srStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(strStorageAccountConnectionString);
                }
                catch(Exception e)
                {
                    Trace.TraceError("Exception occurred when parsing storage account connection string\n{0}\n{1}",
                        strStorageAccountConnectionString, e.Message);
                }

                if(srStorageAccount != null)
                {
                    srCloudQueueClient = srStorageAccount.CreateCloudQueueClient();
                    srBsmQueue = srCloudQueueClient.GetQueueReference(srBsmQueueName);
                
                    try
                    {
                        if(srBsmQueue.CreateIfNotExists())
                        {
                            Trace.TraceInformation("Created Azure BSM queue '{0}'", srBsmQueueName);
                        }
                        else
                        {
                            Trace.TraceInformation("Got reference to existing BSM queue '{0}'", srBsmQueueName);
                        }
                    }
                    catch(Exception e)
                    {
                        Trace.TraceError("Exception occurred when creating queue for inbound BSM bundles\n{0}",
                            e.Message);

                        srBsmQueue = null;
                    }
                }
            }
            
            Trace.TraceInformation("[TRACE] Exiting BsmController::BsmController() static initializer...");
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

        // GET api/bsm
        public IEnumerable<string> Get()
        {
            Trace.TraceInformation("[TRACE] Entering BsmController::Get()...");

            string strWelcomeMessage = 
                String.Format("BsmWebAPI - Last Updated {0} UTC", this.GetLastBuildTimestamp_UTC());

            if(srBsmQueue == null)
            {
                Trace.TraceError("Unable to get queue message count-- Inbound BSM queue not created");
                Trace.TraceInformation("[TRACE] Exiting BsmController::Get()...");
                return new string[]  { strWelcomeMessage };
            }
            else  //BSM queue exists
            {
                // Get the queue message count
                srBsmQueue.FetchAttributes();
                int? messageCount = srBsmQueue.ApproximateMessageCount;
                
                string strQueueMessageCount = 
                    String.Format("[Queue]{0}.ApproximateMessageCount={1}", srBsmQueueName, messageCount);

                Trace.TraceInformation("[TRACE] Exiting BsmController::Get()...");
                return new string[]  { strWelcomeMessage, strQueueMessageCount };
            }
        }

        // POST api/bsm
        public HttpResponseMessage Post([FromBody]BsmBundle bundle)
        {
            Trace.TraceInformation("[TRACE] Entering BsmController::Post(BsmBundle)...");

            if(srBsmQueue == null)
            {
                Trace.TraceError("Unable to add BsmBundle to queue-- Inbound BSM queue not created");
                Trace.TraceInformation("[TRACE] Exiting BsmController::Post(BsmBundle)...");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Inbound BSM queue does not exist");
            }

            if(this.ModelState.IsValid)
            {
                string strQueueMessage = Newtonsoft.Json.JsonConvert.SerializeObject(bundle);

                Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage rMessage = 
                    new Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage(strQueueMessage);

                const int hours = 1;
                const int mins = 0;
                const int secs = 0;
                TimeSpan rTimeToLive = new TimeSpan(hours, mins, secs);
                
                Trace.TraceInformation("Adding BsmBundle message to inbound BSM queue...");
                srBsmQueue.AddMessage(rMessage, rTimeToLive);
                
                Trace.TraceInformation("[TRACE] Exiting BsmController::Post(BsmBundle)...");
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                Trace.TraceError("Unable to add BsmBundle to queue-- BsmBundle message not properly formatted");
                Trace.TraceInformation("[TRACE] Exiting BsmController::Post(BsmBundle)...");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "BsmBundle message not properly formatted");
            }
        }
    }
}
