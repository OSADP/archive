using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using VescoConsole;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            VescoLog vl = new VescoLog();
            Properties props = new Properties();

            DirectoryWatcher watcher = new DirectoryWatcher();
            GmailWatcher gw = new GmailWatcher();
        }
    }
}
