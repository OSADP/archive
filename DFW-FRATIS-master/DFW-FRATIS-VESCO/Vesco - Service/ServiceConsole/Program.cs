using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            GmailWatcher gw = new GmailWatcher();
            gw.Watch();
        }
    }
}
