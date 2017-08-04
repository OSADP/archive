using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;

namespace INCZONE.Forms
{
    public class BaseForm : Form
    {
        protected void _CloseAllMDIForms(Form parentForm)
        {
            Form[] childForms = parentForm.MdiChildren;
            foreach (Form Form in childForms)
            {
                Form.Close();
            }
        }
        
        protected void _OpenForm(Object obj)
        {
            Form form = (Form)obj;
            //_CloseAllMDIForms(form.MdiParent);
            form.WindowState = FormWindowState.Maximized;
            form.Show();
        }
        
        protected string GetConnectionInfo(ConnectionType connType)
        {
            return "NEED TO DO";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr"></param>
        /// <returns>bool</returns>
        protected bool IsValidIp(string addr)
        {
            IPAddress ip;
            bool valid = !string.IsNullOrEmpty(addr) && IPAddress.TryParse(addr, out ip);
            return valid;
        }

    }
}
