using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INCZONE.Forms.Configuration
{
    public partial class EventInfoForm : Form
    {
        int errorCount = 0;

        public EventInfoForm(string info)
        {
            InitializeComponent();
            addMessage(info);
        }

        private void ErrorEventBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        public delegate void StringDelegate(String str);
        public void addMessage(String message)
        {
            if (logBox.InvokeRequired)
                try { logBox.Invoke(new StringDelegate(addMessage), new Object[] { message }); }
                catch { }
            else
            {
                if (!logBox.Text.Equals(""))
                    logBox.AppendText(Environment.NewLine);
                logBox.SelectionColor = Color.Black;
                logBox.AppendText(message);
                if (logBox.Text.Length > 2)
                    logBox.Select(logBox.Text.Length - 1, 1);
                logBox.ScrollToCaret();
            }
        }
    }
}
