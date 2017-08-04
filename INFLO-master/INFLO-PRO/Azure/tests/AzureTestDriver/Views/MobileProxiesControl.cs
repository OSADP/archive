using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AzureTestDriver.BSM;
using AzureTestDriver.I2V;

namespace AzureTestDriver.Views
{
    public partial class MobileProxiesControl : UserControl
    {
        private bool proxiesRunning = false;
        private BindingList<MobileProxy> mobileProxies = new BindingList<MobileProxy>();


        public MobileProxiesControl()
        {
            InitializeComponent();
        }

        private void MobileProxiesControl_Load(object sender, EventArgs e)
        {
            btnStart.Enabled = !proxiesRunning;
            btnStop.Enabled = proxiesRunning;

            updateFromSettings();
        }

        private void updateFromSettings()
        {
            //Get mobile proxy count correct
            while (Properties.Settings.Default.MobileProxiesCount > mobileProxies.Count())
            {
                mobileProxies.Add(new MobileProxy());
            }
            while (Properties.Settings.Default.MobileProxiesCount < mobileProxies.Count())
            {
                MobileProxy last = mobileProxies.Last();
                last.Stop();
                mobileProxies.Remove(last);
            }

            //Ensure mobile proxy settings are correct
            foreach (var mobile in mobileProxies)
            {
                mobile.BsmController.BundleMaxSize = Properties.Settings.Default.MobileBsmBundleMaxSize;
                mobile.BsmController.BundlerTimeout = Properties.Settings.Default.MobileBsmBundlerTimeout;
                mobile.BsmController.MaxSendWorkerCount = Properties.Settings.Default.MobileBsmMaxSendWorkerCount;
                mobile.BsmController.WebApiUrl = Properties.Settings.Default.MobileBsmWebApi;

                mobile.BsmGenerateInterval = Properties.Settings.Default.MobileBsmGenerateInterval;

                mobile.I2VController.WebApiUrl = Properties.Settings.Default.MobileI2VWebApi;
                mobile.I2VPollInterval = Properties.Settings.Default.MobileI2VPollInterval;

                if (proxiesRunning)
                    mobile.Start();
            }

            bsmDisplayControl.SetBsmNetworkControllers(new BindingList<BsmNetworkController>(mobileProxies.Select(x => x.BsmController).ToList()));
            i2VDisplayControl.SetI2VNetworkControllers(new BindingList<I2VNetworkController>(mobileProxies.Select(x => x.I2VController).ToList()));
        }

        public override void Refresh()
        {
            bsmDisplayControl.Refresh();
            i2VDisplayControl.Refresh();

            base.Refresh();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            proxiesRunning = true;

            foreach (var mobile in mobileProxies)
                mobile.Start();

            btnStart.Enabled = !proxiesRunning;
            btnStop.Enabled = proxiesRunning;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            proxiesRunning = false;

            foreach (var mobile in mobileProxies)
                mobile.Stop();

            btnStart.Enabled = !proxiesRunning;
            btnStop.Enabled = proxiesRunning;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            MobileProxySettings settings = new MobileProxySettings();
            settings.ShowDialog();

            updateFromSettings();
        }
    }
}
