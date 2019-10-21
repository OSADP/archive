using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using S22.Imap;

namespace VescoConsole
{
    public class GmailChecker
    {

        public void GetGmail(object source, ElapsedEventArgs e)
        {
            VescoLog.LogEvent("Getting Gmail " + DateTime.Now);

            VescoLog.LogEvent("Creating Imap Client " + DateTime.Now);
            ImapClient client = new ImapClient(
                Properties.gmailImap, 993, Properties.gmailUser, Properties.gmailPassword, AuthMethod.Login, true);
            VescoLog.LogEvent("Finished Creating Imap Client " + DateTime.Now);

            CheckForNewDaily(client);
            CheckForNewFriday(client);
            CheckForNewReOptEmails(client);
            client.Dispose();

            VescoLog.LogEvent("Finished getting the Gmail " + DateTime.Now + Environment.NewLine);
        }

        public void CheckForNewDaily(ImapClient _client)
        {

            // Get the Optimize
            IEnumerable<uint> optUid = _client.Search(
                SearchCondition.Subject(Properties.watchOptSubject)
                .And(SearchCondition.Unseen())
            );

            if (optUid.Count() != 0)
            {
                VescoLog.LogEvent("New Daily Email");

                // The email is here
                Program.shouldWait = true;
                MailMessage optMailMessage = _client.GetMessage(optUid.First());
                Attachment optAttach = optMailMessage.Attachments.First();
                SavePlan(optAttach, Properties.dropOptDir);

                Thread.Sleep(5000); //5 seconds
                VescoLog.LogEvent("Daily attachment placed in drop directory.");
            }
            else
            {
                VescoLog.LogEvent("No New Daily Email");
            }
        }

        public void CheckForNewFriday(ImapClient _client)
        {

            // Get the Optimize
            IEnumerable<uint> optUid = _client.Search(
                SearchCondition.Subject(Properties.watchFriOptSubject)
                .And(SearchCondition.Unseen())
            );

            if (optUid.Count() != 0)
            {
                VescoLog.LogEvent("New Friday Email");

                // The email is here
                Program.shouldWait = true;
                MailMessage optMailMessage = _client.GetMessage(optUid.First());
                Attachment optAttach = optMailMessage.Attachments.First();
                SavePlan(optAttach, Properties.dropOptDir);

                Thread.Sleep(5000); //5 seconds
                VescoLog.LogEvent("Friday attachment placed in drop directory.");
            }
            else
            {
                VescoLog.LogEvent("No New Friday email");
            }
        }

        public void CheckForNewReOptEmails(ImapClient _client)
        {
            // Get the ReOptimize first since it always comes in second
            IEnumerable<uint> reOptUid = _client.Search(
                SearchCondition.Subject(Properties.watchReOptSubject)
                .And(SearchCondition.Unseen())
            );

            if (reOptUid.Count() != 0)
            {
                VescoLog.LogEvent("ReOpt email is here.  Grab the ReOpt Email");

                Program.shouldWait = true;
                // Now grab the executed
                IEnumerable<uint> executedUid = _client.Search(
                    SearchCondition.Subject(Properties.watchExSubject)
                    .And(SearchCondition.Unseen())
                );

                if (executedUid.Count() != 0)
                {
                    VescoLog.LogEvent("Executed email is here.  Grab the Executed Email");

                    // both emails are here, grab the attachments and 
                    // drop them in the directory to be processed
                    MailMessage exMailMessage = _client.GetMessage(executedUid.First());
                    Attachment exAttach = exMailMessage.Attachments.First();
                    SavePlan(exAttach, Properties.dropExDir);

                    MailMessage reOptMailMessage = _client.GetMessage(reOptUid.First());
                    Attachment reOptAttachment = reOptMailMessage.Attachments.First();
                    SavePlan(reOptAttachment, Properties.dropReOptDir);

                    Thread.Sleep(5000); //5 seconds
                    VescoLog.LogEvent("Executed and ReOpt attachments placed in drop directory");
                }
                else
                {
                    // This should never ever happen, but if it does: 
                    // revert the ReOpt to unseen, exit out, and try again in 10 minutes
                    VescoLog.LogEvent("ReOpt Found but no Executed.  What up with that?");
                    _client.RemoveMessageFlags(reOptUid.First(), null, MessageFlag.Seen);
                }
            }
            else
            {
                VescoLog.LogEvent("No New ReOpt emails");
            }
        }

        private void SavePlan(Attachment _attachment, String _dir)
        {
            byte[] allBytes = new byte[_attachment.ContentStream.Length];
            int bytesRead = _attachment.ContentStream.Read(allBytes, 0, (int)_attachment.ContentStream.Length);

            string destinationFile = _dir + _attachment.Name;

            BinaryWriter writer = new BinaryWriter(new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            writer.Write(allBytes);
            writer.Close();
        }
    }
}
