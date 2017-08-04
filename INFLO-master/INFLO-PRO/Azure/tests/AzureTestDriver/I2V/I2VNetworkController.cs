using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace AzureTestDriver.I2V
{
    public class I2VNetworkController : NetworkControllerLog
    {
        public event I2VNetworkMessageReceivedEventHandler MessageReceived;

        public string WebApiUrl { get; set; }
        public bool Active { get { return active; } }

        private HttpClient client = new HttpClient();
        private object activeLock = new object();
        private volatile bool active = false;

        public I2VNetworkController()
        {
            //Set Keep-Alive as default
            client.DefaultRequestHeaders.ConnectionClose = false;

            //Set defaults
            WebApiUrl = "http://posttestserver.com/";
        }

        /// <summary>
        /// Poll Infrustrusture for I2V Messages using Http GET
        /// </summary>
        public void PollInfrustructure()
        {
            lock (activeLock)
            {
                if (!active)
                {
                    active = true;

                    HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, this.WebApiUrl);

                    DateTime startTime = DateTime.Now;

                    client.SendAsync(message).ContinueWith(async (continuation) =>
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
                            this.AddError(continuation.Result.StatusCode.ToString() + ": " + continuation.Result.ReasonPhrase);

                        }
                        else
                        {
                            string results = await continuation.Result.Content.ReadAsStringAsync();
                            this.AddSuccess(results);

                            OnMessageReceived(results);

                            DateTime endTime = DateTime.Now;
                        }

                        active = false;
                    });
                }
            }
        }

        protected virtual void OnMessageReceived(string results)
        {
            if (this.MessageReceived != null)
                this.MessageReceived(this, new I2VNetworkMessageReceivedEventArgs(results));
        }
        
    }

    /// <summary>
    /// Event Handler for when a new I2V Message is received.
    /// </summary>
    /// <param name="sender">Object generating the message</param>
    /// <param name="e">Event arguments including I2V Message</param>
    public delegate void I2VNetworkMessageReceivedEventHandler(object sender, I2VNetworkMessageReceivedEventArgs e);
    /// <summary>
    /// Event Arguments for when a new I2V Message is generated.
    /// </summary>
    public class I2VNetworkMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The I2V Message created by the generator
        /// </summary>
        public string Message { get; private set; }

        public I2VNetworkMessageReceivedEventArgs(string message)
        {
            Message = message;
        }
    }
}
