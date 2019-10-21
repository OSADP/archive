using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S22.Imap;

namespace VescoConsole
{
    public class GmailWatcher
    {
        ImapClient client;
        AutoResetEvent reconnectEvent = new AutoResetEvent(false);
        private Boolean hasExecutedRun;

        public GmailWatcher()
        {
            Watch();
        }

        public void Watch()
        {
            VescoLog.LogEvent(String.Format("Creating Email Watch for {0}", Properties.gmailUser));
            client = new ImapClient(
                Properties.gmailImap, 993, Properties.gmailUser, Properties.gmailPassword, AuthMethod.Login, true);

            // Make sure IDLE is actually supported by the server.
            if (client.Supports("IDLE"))
            {
                VescoLog.LogEvent("Server supports IMAP IDLE");
            }
            else
            {
                VescoLog.LogEvent("Server does NOT support IMAP IDLE");
            }

            // We want to be informed when new messages arrive.
            client.IdleError += client_IdleError;
            client.NewMessage += client_NewMessage;
        }

        private void client_NewMessage(object sender, IdleMessageEventArgs e)
        {
            VescoLog.LogEvent(String.Format("Got a new message, uid = " + e.MessageUID));
            MailMessage mm = e.Client.GetMessage(e.MessageUID);
            VescoLog.LogEvent(String.Format("New email from <{0}>, subject <{1}>, attachments <{2}>", 
                mm.From, mm.Subject, mm.Attachments.Count()));

            String[] distList = Properties.watchEmail.Split(','); 
            
            //if (
            //        mm.From.Address.Equals(Properties.watchEmail, StringComparison.InvariantCultureIgnoreCase)  ||
            //        mm.From.Address.Equals("chuckerin.black@leidos.com", StringComparison.InvariantCultureIgnoreCase)
            //   ) 
            //{
            if (distList.Any(s => s.IndexOf(mm.From.Address, StringComparison.CurrentCultureIgnoreCase) > -1))
            {
                if (mm.Subject.Contains(Properties.watchOptSubject))
                {
                    if (mm.Attachments.Count() > 0)
                    {
                        Attachment attachment = mm.Attachments.First();
                        SavePlan(attachment, Properties.dropOptDir);
                        VescoLog.LogEvent(String.Format("Opt Attachment Name, {0}", attachment.Name));
                        e.Client.AddMessageFlags(e.MessageUID, null, MessageFlag.Seen);
                    }
                }
                else if (mm.Subject.Contains(Properties.watchExSubject) || mm.Subject.Contains(Properties.watchReOptSubject)) 
                {
                    e.Client.RemoveMessageFlags(e.MessageUID, null, MessageFlag.Seen);

                    // wait 5 seconds, sometimes the second message gets hung up.
                    //System.Threading.Thread.Sleep(5000);

                    // Get the Executed
                    IEnumerable<uint> executedUid = e.Client.Search(
                        SearchCondition.Subject(Properties.watchExSubject)
                        .And(SearchCondition.Unseen())
                    );

                    // this should never happen because the executed is always supposed to come first
                    // but if it does exit out and wait for the reOpt email to fire off the event
                    if (executedUid.Count() == 0)
                    {
                        VescoLog.LogEvent("ReOpt Email got delivered first.");
                        // reset the reOpt email message
                        e.Client.RemoveMessageFlags(e.MessageUID, null, MessageFlag.Seen);
                    }
                    else
                    {

                        // Get the ReOpt
                        IEnumerable<uint> reOptUid = e.Client.Search(
                            SearchCondition.Subject(Properties.watchReOptSubject)
                            .And(SearchCondition.Unseen())
                        );

                        // the reopt isn't there, yet so it will have 
                        if (reOptUid.Count() == 0)
                        {
                            VescoLog.LogEvent("Executed email is there but the reOpt isn't.");
                            // reset the executed email message
                            e.Client.RemoveMessageFlags(executedUid.First(), null, MessageFlag.Seen);
                        }
                        // Get both attachments from the mail messages and drop them in the watched directories
                        else 
                        {
                            MailMessage exMailMessage = e.Client.GetMessage(executedUid.First());
                            Attachment exAttach = exMailMessage.Attachments.First();
                            SavePlan(exAttach, Properties.dropExDir);
                            VescoLog.LogEvent(String.Format("Ex Attachment Name, {0}", exAttach.Name));

                            hasExecutedRun = true;

                            MailMessage reOptMailMessage = e.Client.GetMessage(reOptUid.First());
                            Attachment reOptAttachment = mm.Attachments.First();
                            SavePlan(reOptAttachment, Properties.dropReOptDir);
                            VescoLog.LogEvent(String.Format("ReOpt Attachment Name, {0}", reOptAttachment.Name));

                            VescoLog.LogEvent("Number of Emails -> " + (executedUid.Count() + reOptUid.Count()));
                        }
                    }
                }
                else
                {
                    VescoLog.LogEvent("Don't Process Message");
                    e.Client.RemoveMessageFlags(e.MessageUID, null, MessageFlag.Seen);
                }
            }
            else 
            {
                VescoLog.LogEvent("Don't Process Message");
                e.Client.RemoveMessageFlags(e.MessageUID, null, MessageFlag.Seen);
            }
        }

        // this gets called sometimes because the executed and reopt emails 
        // come in at the same time and the executed portion never gets called
        private void grabLatestExecuted(IdleMessageEventArgs e)
        {
            uint u = e.MessageUID - 1;
            MailMessage mm = e.Client.GetMessage(u);
            Attachment attachment = mm.Attachments.First();
            SavePlan(attachment, Properties.dropExDir);
            VescoLog.LogEvent(String.Format("Ex Attachment Name, {0}", attachment.Name));
            e.Client.AddMessageFlags(e.MessageUID, null, MessageFlag.Seen);
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

        //private void client_IdleError(object sender, IdleErrorEventArgs e)
        //{
        //    VescoLog.LogEvent("An error occurred while idling: ");
        //    Exception idleException = e.Exception;
        //    string idleErrorMessage = string.Empty;
        //    while (idleException != null)
        //    {
        //        idleErrorMessage += idleException.ToString();
        //        idleException = idleException.InnerException;
        //    }
        //    VescoLog.LogEvent(idleErrorMessage);
        //    try
        //    {
        //        VescoLog.LogEvent("Trying to reset");
        //        reconnectEvent.Set();
        //        VescoLog.LogEvent("Reconnect successful");

        //        VescoLog.LogEvent("Trying to create events");
        //        e.Client.IdleError += client_IdleError;
        //        e.Client.NewMessage += client_NewMessage;
        //        VescoLog.LogEvent("Event creation successful");
        //    }
        //    catch (Exception ex)
        //    {
        //        Exception reconnectException = ex;
        //        string reconnectErrorMessage = string.Empty;
        //        while (reconnectException != null)
        //        {
        //            reconnectErrorMessage += reconnectException.ToString();
        //            reconnectException = reconnectException.InnerException;
        //        }
        //        VescoLog.LogEvent("An error occurred while resetting or creating events:");
        //        VescoLog.LogEvent(reconnectErrorMessage);
        //    }
        //}

        private void client_IdleError(object sender, IdleErrorEventArgs e)
        {
            VescoLog.LogEvent("An error occurred while idling at time: " + DateTime.Now);
            Exception idleException = e.Exception;
            string idleErrorMessage = string.Empty;
            while (idleException != null)
            {
                idleErrorMessage += idleException.ToString();
                idleException = idleException.InnerException;
            }
            VescoLog.LogEvent(idleErrorMessage);

            try
            {
                VescoLog.LogEvent("Trying to dispose");
                client.Dispose();
                client = null;
                VescoLog.LogEvent("Dispose successful");

                VescoLog.LogEvent("Trying to call Watch()");
                Watch();
                VescoLog.LogEvent("Call to Watch() successful");
            }
            catch (Exception ex)
            {
                Exception reconnectException = ex;
                string reconnectErrorMessage = string.Empty;
                while (reconnectException != null)
                {
                    reconnectErrorMessage += reconnectException.ToString();
                    reconnectException = reconnectException.InnerException;
                }
                VescoLog.LogEvent("An error occurred while disposing or watching:");
                VescoLog.LogEvent(reconnectErrorMessage);
            }        
        }
    }
}
