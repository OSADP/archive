using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NTRIP
{
    public class NTRIPConfiguration
    {
        public static void StoreConfiguration(string HostIP, int HostPort, string Username, string Password)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;

                Config config = new Config()
                {
                    HostIP = HostIP,
                    Password = Password,
                    Port = HostPort,
                    Username = Username
                };

                XmlSerializer xml = new XmlSerializer(typeof(Config));
                xml.Serialize(File.OpenWrite(path + "\\config.xml"), config);
            }
            catch (Exception e)
            {
                Console.WriteLine("Configuration create caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
        }

        public static string GetMd5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
