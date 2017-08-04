using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using AzureTestDriver.BSM;
using AzureTestDriver.I2V;

namespace AzureTestDriver
{
    /// <summary>
    /// Class defines a mobile proxy, creating the network traffic of a single mobile device in the INFLO system.
    /// </summary>
    public class MobileProxy
    {
        /// <summary>
        /// Mobile Proxy's BSM Network Controller. Generates HTTP POST requests.
        /// </summary>
        public BsmNetworkController BsmController { get; private set; }
        /// <summary>
        /// Proxy's I2V Network Controller.  Generates HTTP GET requests.
        /// </summary>
        public I2VNetworkController I2VController {get; private set;}
        
        /// <summary>
        /// Rate at which the Mobile Proxy generates new BSM messages to be posted using the BSM Network Controller.
        /// </summary>
        public uint BsmGenerateInterval { get { return BsmGenerator.GenerateInterval; } set { BsmGenerator.GenerateInterval = value; } }
        /// <summary>
        /// Rate at which the Mobile Proxy polls the infrustructure for new messages.
        /// </summary>
        public uint I2VPollInterval { get { return (uint)I2VPollTimer.Interval; } set { I2VPollTimer.Interval = value; } }

        private IBsmGenerator BsmGenerator;
        private Timer I2VPollTimer = new Timer(1000);

        public MobileProxy()
        {
            BsmController = new BsmNetworkController(new BsmBundleFormatterJson());
            BsmGenerator = new BsmGeneratorFromFile();
            BsmGenerator.MessageGenerated += BsmGenerator_MessageGenerated;

            I2VController = new I2VNetworkController();
            I2VPollTimer.AutoReset = true;
            I2VPollTimer.Elapsed += I2VPollTimer_Elapsed;
        }

        void I2VPollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            I2VController.PollInfrustructure();
        }

        public void Start()
        {
            BsmGenerator.Start();
            I2VPollTimer.Start();
        }

        public void Stop()
        {
            BsmGenerator.Stop();
            I2VPollTimer.Stop();
        }


        void BsmGenerator_MessageGenerated(object sender, BsmMessageGeneratedEventArgs e)
        {
            //Add new message to controller.  Controller will send message when BSM bundle is ready.
            BsmController.AddBsmMessage(e.Message);
        }
    }
}
