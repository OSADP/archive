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
    public partial class OptimizeType : Form
    {
        Dictionary<string, string> Properties;
        int carrier;

        public OptimizeType()
        {
            InitializeComponent();
            Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
        }

        private void btnInitialOptimization_Click(object sender, EventArgs e)
        {
            carrier = Convert.ToInt16(Properties["carrier"]);
            if (carrier == 1)
            {
                Associated ass = new Associated();
                ass.Show();
            }
            else if (carrier == 2)
            {
                SouthWest sw = new SouthWest();
                sw.Show();
            }
            this.Hide();
        }

        private void btnReOptimization_Click(object sender, EventArgs e)
        {
            ReOptimizer ex = new ReOptimizer();
            ex.Show();
            this.Hide();
        }

        private void OptimizeType_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties["skip"] = "false";
            Util.saveProperties(Properties);
        }
    }
}
