using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Forms;
using log4net;
using log4net.Config;

namespace INCZONE
{
    static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            XmlConfigurator.Configure();
            Application.Run(new SplashForm());
            try
            {
                Application.Run(new IncZoneMDIParent());
            }
            catch (Exception ex)
            {
                log.Error("Application RUN Exception", ex);
            }
        }
    }
}
