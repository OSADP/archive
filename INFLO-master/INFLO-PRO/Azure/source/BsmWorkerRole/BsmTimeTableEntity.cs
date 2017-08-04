/*!
    @file         BsmWorkerRole/BsmTimeTableEntity.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsmWorkerRole
{
    public class BsmTimeTableEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public BsmTimeTableEntity()
        {
            this.PartitionKey = "";
            this.RowKey = Guid.NewGuid().ToString();
        }

        public void SetQueueAverageInsertTime(IEnumerable<DateTimeOffset?> times)
        {
            TimeStamp01_QueueAverageInsert = times
                .Where(x => x != null)
                .Select(x => (DateTimeOffset)x)
                .Average(x => ConvertTimeToMillis(x));
        }

        public void SetQueueExtractTime(DateTimeOffset? time)
        {
            TimeStamp02_QueueExtract = ConvertTimeToMillis(time);
        }

        public void SetDeserializeCompleteTime(DateTimeOffset? time)
        {
            TimeStamp03_DeserializeComplete = ConvertTimeToMillis(time);
        }

        public void SetQueueDeleteCompleteTime(DateTimeOffset? time)
        {
            TimeStamp04_QueueDeleteComplete = ConvertTimeToMillis(time);
        }

        public void SetDbCommitEndTime(DateTimeOffset? time)
        {
            TimeStamp05_DbCommitEnd = ConvertTimeToMillis(time);
        }

        public int Stat_NumberQueueMessagesProcessed { get; set; }
        public int? Stat_ApproximateQueueLength { get; set; }
        public int Stat_BsmsExtracted { get; set; }

        public double TimeStamp01_QueueAverageInsert { get; set; }
        public double TimeStamp02_QueueExtract { get; set; }
        public double TimeStamp03_DeserializeComplete { get; set; }
        public double TimeStamp04_QueueDeleteComplete { get; set; }
        public double TimeStamp05_DbCommitEnd { get; set; }

        public double ElapsedTime_TimeInQueue { get { return TimeStamp02_QueueExtract - TimeStamp01_QueueAverageInsert; } set { } }
        public double ElapsedTime_TimeToProcess { get { return TimeStamp04_QueueDeleteComplete - TimeStamp02_QueueExtract; } set { } }
        public double ElapsedTime_TimeToDbCommit { get { return TimeStamp05_DbCommitEnd - TimeStamp03_DeserializeComplete; } set { } }
        public double ElapsedTime_TimeEndToEnd { get { return TimeStamp05_DbCommitEnd - TimeStamp01_QueueAverageInsert; } set { } }

        public static double ConvertTimeToMillis(DateTimeOffset? time)
        {
            if (time != null)
                return ((DateTimeOffset)time).ToUniversalTime().TimeOfDay.TotalMilliseconds;

            return 0;
        }
    }
}
