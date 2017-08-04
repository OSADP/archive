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
using AzureTestDriver.BSM;

namespace AzureTestDriver.Views
{
    public partial class BsmDisplayControl : UserControl
    {
        BindingList<BsmNetworkController> networkControllers;

        public BsmDisplayControl()
        {
            InitializeComponent();
        }

        public void SetBsmNetworkControllers(BindingList<BsmNetworkController> controllers)
        {
            networkControllers = controllers;
            gridBsmStatus.DataSource = controllers;
        }

        public override void Refresh()
        {
            gridBsmStatus.Refresh();

            uint successCount = 0;
            uint errorCount = 0;
            uint activeWorkerCount = 0;
            double estimagedCompletionTime = 0;
            double estimatedSuccessRate = 0;

            foreach (var controller in networkControllers)
            {
                successCount += controller.SuccessCount;
                errorCount += controller.ErrorCount;
                activeWorkerCount += controller.ActiveWorkerCount;
                estimagedCompletionTime += controller.EstimatedCompletionTime;
                estimatedSuccessRate += controller.EstimatedSuccessRate;
            }
            estimagedCompletionTime /= networkControllers.Count();

            lblTotalSuccessCount.Text = successCount.ToString();
            lblTotalErrorCount.Text = errorCount.ToString();
            lblActiveWorkerCount.Text = activeWorkerCount.ToString();
            lblEstimatedCompletionTime.Text = estimagedCompletionTime.ToString("f2");
            lblEstimatedSuccessRate.Text = estimatedSuccessRate.ToString("f2");

            base.Refresh();
        }

        private void gridBsmStatus_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gridBsmStatus.Columns[e.ColumnIndex].Name == "ViewResponses")
            {
                NetworkControllerLogViewer logViewer = new NetworkControllerLogViewer(networkControllers.ElementAt(e.RowIndex));
                logViewer.ShowDialog();
            }
        }

        private void gridBsmStatus_Paint(object sender, PaintEventArgs e)
        {
            foreach (DataGridViewRow r in gridBsmStatus.Rows)
            {
                ((DataGridViewButtonCell)r.Cells["ViewResponses"]).Value = "View Responses";
            }
        }

        private void BsmDisplayControl_Load(object sender, EventArgs e)
        {
            EstimatedCompletionTime.DefaultCellStyle.Format = "f2";


            Type dgvType = gridBsmStatus.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                  BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(gridBsmStatus, true, null);
        }


    }
}
