using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AzureTestDriver.BSM;

namespace AzureTestDriver
{
    public partial class ControlPanel : Form
    {
        public ControlPanel()
        {
            InitializeComponent();
        }

        private void ControlPanel_Load(object sender, EventArgs e)
        {
            refreshTimer.Start();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            mobileProxiesControl.Refresh();

            refreshTimer.Start();
        }

        /*private void updateFromSettings()
        {
            updateMobileProxySettings();

            bsmDisplayControl1.SetBsmNetworkControllers(new BindingList<BsmNetworkController>(mobileProxies.Select(x => x.BsmController).ToList()));
        }

        private void updateMobileProxySettings()
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

                if (mobileProxiesRunning)
                    mobile.Start();
            }
        }*/
    }
}
