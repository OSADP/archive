using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vesco
{
    public partial class CarrierImport : Form
    {

        private Dictionary<string, string> Properties;
        
        public CarrierImport()
        {
            InitializeComponent();
            Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
        }

        private void btnAssociated_Click(object sender, EventArgs e)
        {
            Properties["carrier"] = "1";
            if (checkBox1.Checked)
            {
                Properties["skip"] = "true";
            }
            Util.saveProperties(Properties);
            Associated ass = new Associated();
            ass.Show();
            this.Hide();
        }

        private void btnSouthwest_Click(object sender, EventArgs e)
        {
            Properties["carrier"] = "2";
            if (checkBox1.Checked)
            {
                Properties["skip"] = "true";
            }
            Util.saveProperties(Properties);
            SouthWest sw = new SouthWest();
            sw.Show();
            this.Hide();
        }

        private void CarrierImport_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnAssociated.PerformClick();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }
    }
}
