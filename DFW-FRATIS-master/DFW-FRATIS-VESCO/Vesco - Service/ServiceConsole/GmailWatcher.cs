using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S22.Imap;

namespace ServiceConsole
{
    public class GmailWatcher
    {
        private const string gmailHost = "smtp.gmail.com";
        private const string gmailImap = "imap.gmail.com";
        private const int gmailPort = 587;
        private const string gmailUser = "fratis.saic@gmail.com";
        private const string gmailPassword = "FRAT!Ssaic262";

        private const string fromEmail = "fratis.saic@gmail.com";
        private const string fromName = "Fratis Smith";

        private const string toEmail = "chuckerin.black@leidos.com";
        private const string toName = "Chuckerin Black";

        private const string watchEmail = "noreply@associatedcarriers.us";
        private const string watchSubject = "Fratis Optimization Report [SAIC OPTIMIZATION";

        private const string testSubject = "Luke Skywal";

        static AutoResetEvent reconnectEvent = new AutoResetEvent(false);

        public void Watch()
        {
            Console.WriteLine("Creating Watch for {0}", gmailUser);
            using (ImapClient client = new ImapClient(gmailImap, 993, gmailUser, gmailPassword, AuthMethod.Login, true))
            {
                // Make sure IDLE is actually supported by the server.
                if (client.Supports("IDLE"))
                {
                    Console.WriteLine("Server supports IMAP IDLE");
                }
                else
                {
                    Console.WriteLine("Server does NOT support IMAP IDLE");
                }

                // We want to be informed when new messages arrive.
                client.IdleError += client_IdleError;
                client.NewMessage += client_NewMessage;

                Console.ReadLine();
            }
        }

        static void client_NewMessage(object sender, IdleMessageEventArgs e)
        {
            Console.WriteLine("Got a new message, uid = " + e.MessageUID);
            MailMessage mm = e.Client.GetMessage(e.MessageUID);
            Console.WriteLine("New emmail from <{0}>, subject <{1}>, attachments <{2}>", mm.From, mm.Subject, mm.Attachments.Count());
//            if (mm.From.Equals(watchEmail) && mm.Subject.Contains(watchSubject))
            if (mm.From.Address.Equals(toEmail, StringComparison.InvariantCultureIgnoreCase) && mm.Subject.Contains(testSubject))
            {
                Console.WriteLine("Process Message");
                e.Client.AddMessageFlags(e.MessageUID, null, MessageFlag.Seen);
            }
            else 
            {
                Console.WriteLine("Don't Process Message");
                e.Client.RemoveMessageFlags(e.MessageUID, null, MessageFlag.Seen);
            }

        }

        static void client_IdleError(object sender, IdleErrorEventArgs e)
        {
            Console.Write("An error occurred while idling: ");
            Console.WriteLine(e.Exception.Message);

            reconnectEvent.Set();
        }

    }
}
