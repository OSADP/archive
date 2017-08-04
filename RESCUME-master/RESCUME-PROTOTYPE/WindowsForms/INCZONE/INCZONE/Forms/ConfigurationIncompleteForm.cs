using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INCZONE.Forms
{
    public partial class ConfigurationIncompleteForm : Form
    {
        public ConfigurationIncompleteForm(IncZoneMDIParent incZoneMDIParent)
        {
            InitializeComponent();
            this.MdiParent = incZoneMDIParent;
        }
    }
}
