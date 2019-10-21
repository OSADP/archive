using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace VescoConsole
{
    class EmailHelper
    {
        public static void sendGmail(String _subject, String _body, Boolean hasAttachment)
        {
            var fromAddress = new MailAddress(Properties.fromEmail, Properties.fromName);

            var smtp = new SmtpClient
            {
                Host = Properties.gmailHost,
                Port = Properties.gmailPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Properties.gmailUser, Properties.gmailPassword),
                Timeout = 20000
            };

            using (var message = new MailMessage()
            {
                Subject = _subject,
                Body = _body,
            })
            {
                // Go through distribution list
                String[] distList = Properties.distList.Split(',');
                foreach (string s in distList)
                {
                    message.To.Add(s);
                }

                System.Threading.Thread.Sleep(10000);

                message.From = fromAddress;
                if (hasAttachment)
                {
                    Attachment attachment = new Attachment(Properties.attachmentPath);
                    message.Attachments.Add(attachment);
                }

                try
                {
                    smtp.Send(message);
                    VescoLog.LogEvent("SMTP Through Attach Gmail Success");

                }
                catch (Exception ex)
                {
                    Exception ex2 = ex;
                    string errorMessage = string.Empty;
                    while (ex2 != null)
                    {
                        errorMessage += ex2.ToString();
                        ex2 = ex2.InnerException;
                    }
                    VescoLog.LogEvent("Dude, you're probably plugged in to the VPN!  Shut that shit down!");
                    VescoLog.LogEvent(ex.StackTrace);
                    VescoLog.LogEvent(errorMessage);
                }
            }
        }
    }
}
