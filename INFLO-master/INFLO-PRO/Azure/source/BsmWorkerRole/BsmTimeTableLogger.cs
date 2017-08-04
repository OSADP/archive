/*!
    @file         BsmWorkerRole/BsmTimeTableLogger.cs
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
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BsmWorkerRole
{
    class BsmTimeTableLogger
    {
        private static CloudTableClient srCloudTableClient;
        private static CloudTable srBsmTimeTable;
        
        private static BsmTimeTableEntity srCurrentBsmTimeTableEntry = new BsmTimeTableEntity();

        private static bool sEnabled = false;
        public static bool Enabled { get { return sEnabled; } set { sEnabled = value && srBsmTimeTable != null; } }

        public static int MinimalLoggedElapsedTime { get; set; }

        public static void Initialize(CloudStorageAccount storageAccount, string bsmTimeTableName)
        {
            srCloudTableClient = storageAccount.CreateCloudTableClient();
            srBsmTimeTable = srCloudTableClient.GetTableReference(bsmTimeTableName);

            try
            {
                if (srBsmTimeTable.CreateIfNotExists())
                {
                    Trace.TraceInformation("Created Azure Table '{0}'", bsmTimeTableName);
                }
                else
                {
                    Trace.TraceInformation("Got reference to existing BSM Time Table '{0}'", bsmTimeTableName);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception occurred when creating BSM Time Table\n{0}",
                    e.Message);

                srBsmTimeTable = null;
            }
        }

        public static void StartNewLogEntry(CloudQueue queue, IEnumerable<CloudQueueMessage> messages)
        {
            if (sEnabled)
            {
                srCurrentBsmTimeTableEntry = new BsmTimeTableEntity();
                srCurrentBsmTimeTableEntry.SetQueueAverageInsertTime(messages.Select(x => x.InsertionTime));
                srCurrentBsmTimeTableEntry.SetQueueExtractTime(DateTimeOffset.Now);
                srCurrentBsmTimeTableEntry.Stat_NumberQueueMessagesProcessed = messages.Count();

                queue.FetchAttributes();
                srCurrentBsmTimeTableEntry.Stat_ApproximateQueueLength = queue.ApproximateMessageCount;
            }
        }

        public static void SetDeserializationComplete()
        {
            if (sEnabled)
                srCurrentBsmTimeTableEntry.SetDeserializeCompleteTime(DateTimeOffset.Now);
        }

        public static void SetQueueDeleteComplete()
        {
            if (sEnabled)
                srCurrentBsmTimeTableEntry.SetQueueDeleteCompleteTime(DateTimeOffset.Now);
        }

        public static void SubmitLogEntry(int extractedBsmCount)
        {
            if (sEnabled)
            {
                srCurrentBsmTimeTableEntry.Stat_BsmsExtracted = extractedBsmCount;
                srCurrentBsmTimeTableEntry.SetDbCommitEndTime(DateTimeOffset.Now);

                if (srCurrentBsmTimeTableEntry.ElapsedTime_TimeEndToEnd > MinimalLoggedElapsedTime)
                    srBsmTimeTable.ExecuteAsync(TableOperation.Insert(srCurrentBsmTimeTableEntry));
            }
        }

    }
}
