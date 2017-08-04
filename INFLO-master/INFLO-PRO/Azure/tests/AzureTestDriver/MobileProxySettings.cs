using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzureTestDriver
{
    public partial class MobileProxySettings : Form
    {
        public MobileProxySettings()
        {
            InitializeComponent();
        }

        private void MobileProxySettings_Load(object sender, EventArgs e)
        {
            this.txtProxyCount.Text = Properties.Settings.Default.MobileProxiesCount.ToString();

            this.txtBSMBerFileLocation.Text = Properties.Settings.Default.BSMBerFileLocation;
            this.txtBSMWebAPIUrl.Text = Properties.Settings.Default.MobileBsmWebApi;
            this.txtGenerateInterval.Text = Properties.Settings.Default.MobileBsmGenerateInterval.ToString();
            this.txtMaxWorkerCount.Text = Properties.Settings.Default.MobileBsmMaxSendWorkerCount.ToString();
            this.txtBundlerMaxSize.Text = Properties.Settings.Default.MobileBsmBundleMaxSize.ToString();
            this.txtBundlerTimeout.Text = Properties.Settings.Default.MobileBsmBundlerTimeout.ToString();

            this.txtI2VWebAPIUrl.Text = Properties.Settings.Default.MobileI2VWebApi;
            this.txtI2VPollPeriod.Text = Properties.Settings.Default.MobileI2VPollInterval.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.MobileProxiesCount = uint.Parse(this.txtProxyCount.Text);
                Properties.Settings.Default.BSMBerFileLocation = this.txtBSMBerFileLocation.Text;
                Properties.Settings.Default.MobileBsmWebApi = this.txtBSMWebAPIUrl.Text;
                Properties.Settings.Default.MobileBsmGenerateInterval = uint.Parse(this.txtGenerateInterval.Text);
                Properties.Settings.Default.MobileBsmMaxSendWorkerCount = uint.Parse(this.txtMaxWorkerCount.Text);
                Properties.Settings.Default.MobileBsmBundleMaxSize = uint.Parse(this.txtBundlerMaxSize.Text);
                Properties.Settings.Default.MobileBsmBundlerTimeout = uint.Parse(this.txtBundlerTimeout.Text);

                Properties.Settings.Default.MobileI2VWebApi = this.txtI2VWebAPIUrl.Text;
                Properties.Settings.Default.MobileI2VPollInterval = uint.Parse(this.txtI2VPollPeriod.Text);
                
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Parse Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelectBsmBerFileLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbDialog = new FolderBrowserDialog();
            fbDialog.SelectedPath = this.txtBSMBerFileLocation.Text;
            fbDialog.ShowDialog();
            this.txtBSMBerFileLocation.Text = fbDialog.SelectedPath;
        }

    }
}
