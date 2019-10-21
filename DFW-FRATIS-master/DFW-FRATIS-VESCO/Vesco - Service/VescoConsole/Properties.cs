using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VescoConsole
{
    public class Properties
    {

        public static string gmailHost;
        public static string gmailImap;
        public static int gmailPort;
        public static string gmailUser;
        public static string gmailPassword;
        public static string fromEmail;
        public static string fromName;
        public static string toEmail;
        public static string toName;
        public static string watchEmail;
        public static string watchOptSubject;
        public static string watchFriOptSubject;
        public static string watchExSubject;
        public static string watchReOptSubject;
        public static string distList;
        public static int emailCheckInterval;
        public static string dropOptDir;
        public static string dropExDir;
        public static string dropReOptDir;
        public static string procDir;
        public static string errDir;
        public static string templateDir;
        public static string attachmentPath;

        public Properties() 
        {
            ReadSettings();
        }

        private void ReadSettings()
        {
            Dictionary<string, string> Properties = Util.GetProperties(
                AppDomain.CurrentDomain.BaseDirectory + "\\settings.properties");
            gmailHost = Properties["gmailHost"];
            gmailImap = Properties["gmailImap"];
            gmailPort = Convert.ToInt16(Properties["gmailPort"]);
            gmailUser = Properties["gmailUser"];
            gmailPassword = Properties["gmailPassword"];
            fromEmail = Properties["fromEmail"];
            fromName = Properties["fromName"];
            toEmail = Properties["toEmail"];
            toName = Properties["toName"];
            watchEmail = Properties["watchEmail"];
            watchOptSubject = Properties["watchOptSubject"];
            watchFriOptSubject = Properties["watchFriOptSubject"];
            watchExSubject = Properties["watchExSubject"];
            watchReOptSubject = Properties["watchReOptSubject"];
            distList = Properties["distList"];
            emailCheckInterval = Convert.ToInt32(Properties["emailCheckInterval"]);
            dropOptDir = Properties["dropOptDir"];
            dropExDir = Properties["dropExDir"];
            dropReOptDir = Properties["dropReOptDir"];
            procDir = Properties["procDir"];
            errDir = Properties["errDir"];
            templateDir = Properties["templateDir"];
            attachmentPath = Properties["attachmentPath"];
        }
    }
}
