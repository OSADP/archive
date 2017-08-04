using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureTestDriver
{
    public class NetworkControllerLog
    {
        private int RECENT_RETENTION_COUNT = 0;

        public uint ErrorCount { get; private set; }
        public List<string> RecentErrors = new List<string>();

        public uint SuccessCount { get; private set; }
        public List<string> RecentSuccesses = new List<string>();

        protected void AddError(string errorDescription)
        {
            ErrorCount++;
            if (RECENT_RETENTION_COUNT != 0 && RecentErrors.Count >= RECENT_RETENTION_COUNT)
                RecentErrors.RemoveAt(0);

            RecentErrors.Add(errorDescription);
        }

        protected void AddSuccess(string successDescription)
        {
            SuccessCount++;

            if (RECENT_RETENTION_COUNT != 0 && RecentSuccesses.Count >= RECENT_RETENTION_COUNT)
                RecentSuccesses.RemoveAt(0);

            RecentSuccesses.Add(successDescription);
        }
    }

}
