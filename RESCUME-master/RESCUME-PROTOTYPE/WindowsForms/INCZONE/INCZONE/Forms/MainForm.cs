using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;
using log4net;

namespace INCZONE.Forms
{
    public partial class MainForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        public MainForm(IncZoneMDIParent form)
        {
            this.MdiParent = form;
            InitializeComponent();

            ((IncZoneMDIParent)form).RequestButtonStatusChange += RequestButtonStatusChange;

            if (IncZoneMDIParent.AppStarted)
            {
                StartApp.Enabled = false;
                StopApp.Enabled = true;
            }
            else
            {
                StartApp.Enabled = true;
                StopApp.Enabled = false;
            }
        }

        private void RequestButtonStatusChange(bool status)
        {
            log.Debug("MainForm Status Changed");
            if (status)
            {
                StartApp.Enabled = false;
                StopApp.Enabled = true;
            }
            else
            {
                StartApp.Enabled = true;
                StopApp.Enabled = false;
            }            
        }

        private void StartApp_Click(object sender, EventArgs e)
        {
            //IncZoneMDIParent._ReconnectCount = 0;
            ((IncZoneMDIParent)this.MdiParent)._StartIncZone();
        }

        private void StopApp_Click(object sender, EventArgs e)
        {
            //IncZoneMDIParent._ReconnectCount = 0;
            ((IncZoneMDIParent)this.MdiParent)._StopIncZone(true);
        }

    }
}

