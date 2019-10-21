using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace VescoConsole
{
    class Program
    {

        public static Boolean shouldWait;

        static void Main(string[] args)
        {
            VescoLog vl = new VescoLog();
            Properties props = new Properties();

            VescoLog.LogEvent("Starting up Vesco Console at " + DateTime.Now);

            DirectoryWatcher watcher = new DirectoryWatcher();

            GmailChecker gmailChecker = new GmailChecker();
            gmailChecker.GetGmail(null, null);

            //Timer myTimer = new Timer();
            //myTimer.Elapsed += new ElapsedEventHandler(gmailChecker.GetGmail);
            //myTimer.Interval = Properties.emailCheckInterval * 60000;
            //myTimer.Start();

            //Timer t = new Timer();
            //t.Interval = 60000; //In milliseconds here
            //t.AutoReset = true; //Stops it from repeating
            //t.Elapsed += new ElapsedEventHandler(TimerElapsed);
            //t.Start();

            if (shouldWait)
            {
                Thread.Sleep(15 * 60000); //15 minutes
            }

//            Console.ReadLine();
        }

        static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            VescoLog.LogEvent("Wait just one minute");
        }
    }
}
