using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VescoConsole
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
    }
}
