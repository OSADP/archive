using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using AzureTestDriver.I2V;

namespace AzureTestDriver.Views
{
    public partial class I2VDisplayControl : UserControl
    {
        BindingList<I2VNetworkController> networkControllers;

        public I2VDisplayControl()
        {
            InitializeComponent();
        }

        public void SetI2VNetworkControllers(BindingList<I2VNetworkController> controllers)
        {
            networkControllers = controllers;
            gridI2VStatus.DataSource = controllers;
        }

        public override void Refresh()
        {
            gridI2VStatus.Refresh();

            uint successCount = 0;
            uint errorCount = 0;
            uint activeWorkerCount = 0;

            foreach (var controller in networkControllers)
            {
                successCount += controller.SuccessCount;
                errorCount += controller.ErrorCount;
                activeWorkerCount += (uint)(controller.Active ? 1 : 0);
            }

            lblTotalSuccessCount.Text = successCount.ToString();
            lblTotalErrorCount.Text = errorCount.ToString();
            lblActiveWorkerCount.Text = activeWorkerCount.ToString();

            base.Refresh();
        }

        private void gridI2VStatus_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gridI2VStatus.Columns[e.ColumnIndex].Name == "ViewResponses")
            {
                NetworkControllerLogViewer logViewer = new NetworkControllerLogViewer(networkControllers.ElementAt(e.RowIndex));
                logViewer.ShowDialog();
            }
        }

        private void gridI2VStatus_Paint(object sender, PaintEventArgs e)
        {
            foreach (DataGridViewRow r in gridI2VStatus.Rows)
            {
                ((DataGridViewButtonCell)r.Cells["ViewResponses"]).Value = "View Responses";
            }
        }

        private void I2VDisplayControl_Load(object sender, EventArgs e)
        {
            Type dgvType = gridI2VStatus.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                  BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(gridI2VStatus, true, null);
        }
    }
}
