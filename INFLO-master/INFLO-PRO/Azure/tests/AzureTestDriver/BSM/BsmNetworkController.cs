using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Timers;

namespace AzureTestDriver.BSM
{

    /// <summary>
    /// BSM Network Controller receives BSM Messages from application, bundles them, and transmits them via HTTP Post to Web URL.
    /// 
    /// </summary>
    public class BsmNetworkController : NetworkControllerLog
    {
        /// <summary> Number of completion times to use to get Estimated Completion Time </summary>
        public const uint COMPLETION_TIME_RUNNINGAVERAGE_COUNT = 30;
        /// <summary> Period of time over which to average the Estimated Success Rate </summary>
        public const uint SUCCESS_RATE_AVERAGING_TIME = 5;


        /// <summary> Web URL to POST the BSM Bundle to. </summary>
        public string WebApiUrl { get; set; }
        /// <summary> Maximum size of a bundle.  When size of current bundle reaches this value, the bundle is automatically transmited. </summary>
        public uint BundleMaxSize { get; set; }
        /// <summary> Timeout (in milliseconds) of the bundler.  When this period has passed since the first bsm message was added to the current bundle, the bundle is automatically transmitted. </summary>
        public uint BundlerTimeout { get { return (uint)bundleTimeoutTimer.Interval; } set { bundleTimeoutTimer.Interval = value; } }
        /// <summary> Maximum number of concurrent live Http transactions. </summary>
        public uint MaxSendWorkerCount { get; set; }
        /// <summary> Timeout period of connection/transmissions/reponses for Http transactions. </summary>
        public TimeSpan SendWorkerTransmitTimeout { get { return client.Timeout; } set { client.Timeout = value; } }

        /// <summary> Returns the number of active workers. </summary>
        public uint ActiveWorkerCount { get { return activeWorkerCount; } }
        /// <summary> Estimated Completion Time of the last "COMPLETION_TIME_RUNNINGAVERAGE_COUNT" interactions </summary>
        public double EstimatedCompletionTime
        {
            get
            {
                double results = 0;

                lock(averageCompletionTimesLock)
                {
                    while (averageCompletionTimes.Count > COMPLETION_TIME_RUNNINGAVERAGE_COUNT)
                    {
                        averageCompletionTimes.RemoveAt(0);
                    }

                    foreach (var time in averageCompletionTimes)
                    {
                        results += time;
                    }
                    results /= averageCompletionTimes.Count();
                }

                return results;
            }
        }
        /// <summary> Estimated Success Rate over the last "SUCCESS_RATE_AVERAGING_TIME" seconds </summary>
        public double EstimatedSuccessRate
        {
            get
            {
                double results = 0;

                lock (completionTimesLock)
                {
                    completionTimes.RemoveAll(x => (DateTime.Now - x).TotalSeconds > SUCCESS_RATE_AVERAGING_TIME);

                    if (completionTimes.Count() >= 1)
                    {
                        results = completionTimes.Count() / (DateTime.Now - completionTimes.First()).TotalSeconds;
                    }
                }

                return results;
            }
        }

        private HttpClient client = new HttpClient();
        private List<IBsmMessage> bsmBundle = new List<IBsmMessage>();
        private bool bundleWaiting = false;                                 //Indicates if there is a bundle waiting for a SendWorker to open up.
        private object bundleLock = new object();                           //Locks the bundle during all editing.
        private Timer bundleTimeoutTimer = new Timer();                     //Used to trigger sending a partial bundle after a given timeout period.

        //Objects used to average completion times
        private List<double> averageCompletionTimes = new List<double>();
        private object averageCompletionTimesLock = new object();

        //Objects used to estimate success rate.
        private List<DateTime> completionTimes = new List<DateTime>();
        private object completionTimesLock = new object();

        //Number of active connections
        private volatile uint activeWorkerCount = 0;

        //Formatter used to generate BsmBundles
        private IBsmBundleFormatter formatter;

        
        public BsmNetworkController(IBsmBundleFormatter formatter)
        {
            this.formatter = formatter;
            
            //Set Keep-Alive as default
            client.DefaultRequestHeaders.ConnectionClose = false;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 

            //Timeout timer setup
            bundleTimeoutTimer.AutoReset = false;
            bundleTimeoutTimer.Elapsed += bundleTimeoutTimer_Elapsed;

            //Set IBsmNetworkController defaults
            WebApiUrl = "http://posttestfserver.com/post.php?dir=shadow";
            BundleMaxSize = 10;
            BundlerTimeout = 1000;
            MaxSendWorkerCount = 1;
            SendWorkerTransmitTimeout = new TimeSpan(0, 0, 3);
        }

        /// <summary>
        /// Add a BSM Message to the Bundler.  The Bundler will transmit automatically.
        /// </summary>
        /// <param name="data">Memory Representation of BSM Message</param>
        public void AddBsmMessage(IBsmMessage data)
        {
            lock(bundleLock)
            {
                //Fill ONLY if there is room in the bundle
                if (bsmBundle.Count() < BundleMaxSize)
                {
                    bundleTimeoutTimer.Start();
                    bsmBundle.Add(data);

                    if (bsmBundle.Count() >= BundleMaxSize)
                        SendBundle();
                }
                else { /* Drop Strategy */ }
            }
        }
        



        private void bundleTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendBundle();
        }

        private void SendBundle()
        {
            bundleTimeoutTimer.Stop();
            lock (bundleLock)
            {
                //Limit number of maximum transactions
                if (activeWorkerCount >= MaxSendWorkerCount)
                {
                    bundleWaiting = true;
                }
                else
                {
                    bundleWaiting = false;
                    activeWorkerCount++;

                    //Construct new message & load body 
                    HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, this.WebApiUrl);
                    message.Content = new StringContent(formatter.GetFormattedString(this.bsmBundle), Encoding.UTF8, "application/json");
                    


                    DateTime startTime = DateTime.Now;

                    client.SendAsync(message).ContinueWith((continuation) =>
                    { //This code block runs (asyncronously) when "SendAsync" task is finished.

                        if (continuation.Status == TaskStatus.Faulted)
                        {
                            this.AddError(continuation.Exception.ToString());
                        }
                        else if (continuation.Status == TaskStatus.Canceled)
                        {
                            this.AddError("Cancelled");
                        }
                        else if (!continuation.Result.IsSuccessStatusCode)
                        {
                            this.AddError(continuation.Result.ToString());
                        }
                        else
                        {
                            this.AddSuccess(continuation.Result.ToString());

                            DateTime endTime = DateTime.Now;
                            lock (averageCompletionTimesLock)
                                averageCompletionTimes.Add((endTime - startTime).TotalSeconds);
                            lock (completionTimesLock)
                                completionTimes.Add(endTime);
                        }


                        activeWorkerCount--;
                        if (bundleWaiting) //If there was a bundle waiting, there is now a send worker to use
                            SendBundle();
                    });

                    bsmBundle.Clear();
                }
            }
        }
    }
}
