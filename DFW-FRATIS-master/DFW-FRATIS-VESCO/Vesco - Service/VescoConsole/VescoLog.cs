using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VescoConsole
{
    public class VescoLog
    {
        public static EventLog eventLog1;

        public VescoLog() 
        {
            InitializeLogger();
        }

        private void InitializeLogger()
        {
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("VescoService"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "VescoService", "VescoServiceLog");
            }
            eventLog1.Source = "VescoService";
            eventLog1.Log = "VescoServiceLog";
        }

        public static void LogEvent(string message)
        {
            eventLog1.WriteEntry(message);
            Console.WriteLine(message);
        }
    }
}
