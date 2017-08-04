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
    public partial class NetworkControllerLogViewer : Form
    {
        NetworkControllerLog controllerLog;

        public NetworkControllerLogViewer(NetworkControllerLog log)
        {
            if (log == null)
                throw new NullReferenceException();

            controllerLog = log;

            InitializeComponent();
        }

        private void NetworkControllerLogViewer_Load(object sender, EventArgs e)
        {
            LoadLists();
        }

        private void LoadLists()
        {
            listSuccesses.Items.Clear();
            listErrors.Items.Clear();

            listSuccesses.Items.AddRange(controllerLog.RecentSuccesses.ToArray());
            listErrors.Items.AddRange(controllerLog.RecentErrors.ToArray());
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadLists();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listErrors_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(listErrors.SelectedItem.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            catch { }
        }

        private void listSuccesses_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(listSuccesses.SelectedItem.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            catch { }
        }
    }
}
