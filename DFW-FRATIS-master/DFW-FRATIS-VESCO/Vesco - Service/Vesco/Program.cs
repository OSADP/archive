using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vesco
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Dictionary<string, string> Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
            int carrier = Convert.ToInt16(Properties["carrier"]);
            Boolean skip = Convert.ToBoolean(Properties["skip"]);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (skip)
            {
//                Application.Run(new OptimizeType());
                if (carrier == 1)
                {
                    Application.Run(new Associated());
                }
                else if (carrier == 2)
                {
                    Application.Run(new SouthWest());
                }
            }
            else
            {
                Application.Run(new CarrierImport());
            }
        }

    }
}
