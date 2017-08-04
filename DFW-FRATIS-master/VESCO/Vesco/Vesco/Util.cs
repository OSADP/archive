using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vesco
{
    class Util
    {
        public static Dictionary<string, string> GetProperties(string path)
        {
            string fileData = "";
            using (StreamReader sr = new StreamReader(path))
            {
                fileData = sr.ReadToEnd().Replace("\r", "");
            }
            Dictionary<string, string> Properties = new Dictionary<string, string>();
            string[] kvp;
            string[] records = fileData.Split("\n".ToCharArray());
            foreach (string record in records)
            {
                if (record != String.Empty)
                {
                    kvp = record.Split("=".ToCharArray());
                    Properties.Add(kvp[0], kvp[1]);
                }
            }
            return Properties;
        }

        public static void saveProperties(Dictionary<string, string> Properties)
        {
            File.WriteAllLines(Application.StartupPath + "\\settings.properties",
                Properties.Select(x => x.Key + "=" + x.Value).ToArray());
        }
    }
}
