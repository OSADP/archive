using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vesco
{
    public partial class ReOptimizer : Form
    {
        Dictionary<string, string> Properties;

        public ReOptimizer()
        {
            InitializeComponent();
            Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
        }

        private void btnExLoad_Click(object sender, EventArgs e)
        {
            Associated ass = new Associated();
            ass.Show();
            this.Hide();
        }

        private void ReOptimizer_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties["skip"] = "false";
            Util.saveProperties(Properties);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }
    }
}
