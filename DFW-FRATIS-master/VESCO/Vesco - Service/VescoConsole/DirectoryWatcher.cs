using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace VescoConsole
{
    public class DirectoryWatcher
    {
        private static Associated ass;

        public DirectoryWatcher()
        {
            CreateDropOptWatcher();
            CreateDropExWatcher();
            CreateDropReOptWatcher();

            CreateEmailWatcher();
        }

        private static void CreateDropOptWatcher()
        {
            FileSystemWatcher dropWatcher = new FileSystemWatcher(Properties.dropOptDir);
            //Watch for changes in LastAccess and LastWrite times, and
            //the renaming of files or directories.
            dropWatcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Add event handler.
            dropWatcher.Created += new FileSystemEventHandler(onDropOptCreated);

            // Begin watching.
            dropWatcher.EnableRaisingEvents = true;
            VescoLog.LogEvent(String.Format("Drop Opt Dir Watcher created on {0}", Properties.dropOptDir));
        }

        private static void CreateDropExWatcher()
        {
            FileSystemWatcher dropWatcher = new FileSystemWatcher(Properties.dropExDir);
            //Watch for changes in LastAccess and LastWrite times, and
            //the renaming of files or directories.
            dropWatcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Add event handler.
            dropWatcher.Created += new FileSystemEventHandler(onDropExCreated);

            // Begin watching.
            dropWatcher.EnableRaisingEvents = true;
            VescoLog.LogEvent(String.Format("Drop Ex Dir Watcher created on {0}", Properties.dropExDir));
        }

        private static void CreateDropReOptWatcher()
        {
            FileSystemWatcher dropWatcher = new FileSystemWatcher(Properties.dropReOptDir);
            //Watch for changes in LastAccess and LastWrite times, and
            //the renaming of files or directories.
            dropWatcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Add event handler.
            dropWatcher.Created += new FileSystemEventHandler(onDropReOptCreated);

            // Begin watching.
            dropWatcher.EnableRaisingEvents = true;
            VescoLog.LogEvent(String.Format("Drop ReOpt Dir Watcher created on {0}", Properties.dropReOptDir));
        }

        private static void CreateEmailWatcher()
        {
            FileSystemWatcher emailWatcher = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory + Properties.attachmentPath);
            //Watch for changes in LastAccess and LastWrite times, and
            //the renaming of files or directories.
            emailWatcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Add event handlers
            emailWatcher.Created += new FileSystemEventHandler(onEmailCreated);

            // Begin watching.
            emailWatcher.EnableRaisingEvents = true;

            VescoLog.LogEvent(String.Format("Email Dir Watcher created on {0} {1}", emailWatcher.Path, Environment.NewLine));
        }

        private static void onDropOptCreated(object source, FileSystemEventArgs e)
        {
            string msg = string.Format("File {0} | {1} | {2}",
                                       e.FullPath, e.ChangeType, e.Name);

            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                VescoLog.LogEvent(msg);

                System.Threading.Thread.Sleep(5000);

                ass = new Associated();
                ass.processAssDrop(e.FullPath);

                moveProcessedFile(e.Name, e.FullPath);

                VescoLog.LogEvent("End of OnDropOptCreated");
            }
        }

        private static void onDropExCreated(object source, FileSystemEventArgs e)
        {
            string msg = string.Format("{3}Executed File {0} | {1} | {2}",
                                       e.FullPath, e.ChangeType, e.Name, Environment.NewLine);
            VescoLog.LogEvent(msg);


            if (e.ChangeType == WatcherChangeTypes.Created)
            {

                System.Threading.Thread.Sleep(5000);

                ass = new Associated();
                ass.processExecuted(e.FullPath);

                moveProcessedFile(e.Name, e.FullPath);

                VescoLog.LogEvent("End of OnDropExCreated");
            }
        }

        private static void onDropReOptCreated(object source, FileSystemEventArgs e)
        {
            string msg = string.Format("{3}ReOptimization File {0} | {1} | {2}",
                                       e.FullPath, e.ChangeType, e.Name, Environment.NewLine);


            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                VescoLog.LogEvent(msg);

                System.Threading.Thread.Sleep(5000);

                ass.processAssDrop(e.FullPath);

                moveProcessedFile(e.Name, e.FullPath);

                VescoLog.LogEvent("End of OnDropReOptCreated");
            }
        }


        private static void onEmailCreated(object source, FileSystemEventArgs e)
        {
            string msg = string.Format("File {0} | {1} | {2}",
                                       e.FullPath, e.ChangeType, e.Name);

            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                Properties.attachmentPath = e.FullPath;
                VescoLog.LogEvent(msg);

                //smtpAttachGmail();
                String subject;
                String body;
                if (ass.isExecuted)
                {
                    subject = "Re-optimized plan";
                    body = "Your re-optimized daily plan for today is attached. Please forward any questions/comments to Diane Newton (Leidos) at newtondia@leidos.com.";
                }
                else
                {
                    subject = "Initial optimization";
                    body = "The optimized plan for your next business day is attached. Please forward any questions/comments to Diane Newton (Leidos) at newtondia@leidos.com.";
                }
                EmailHelper.sendGmail(subject, body, true);

                VescoLog.LogEvent("End of onEmailCreated");
            }
        }

        //public static void smtpAttachGmail()
        //{
        //    var fromAddress = new MailAddress(Properties.fromEmail, Properties.fromName);
        //    string subject;
        //    string body;

        //    if (ass.isExecuted)
        //    {
        //        subject = "Re-optimized plan";
        //        body = "Your re-optimized daily plan for today is attached. Please forward any questions/comments to Diane Newton (Leidos) at newtondia@leidos.com.";
        //    }
        //    else
        //    {
        //        subject = "Initial optimization";
        //        body = "The optimized plan for your next business day is attached. Please forward any questions/comments to Diane Newton (Leidos) at newtondia@leidos.com.";
        //    }

        //    var smtp = new SmtpClient
        //    {
        //        Host = Properties.gmailHost,
        //        Port = Properties.gmailPort,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential(Properties.gmailUser, Properties.gmailPassword),
        //        Timeout = 20000
        //    };

        //    using (var message = new MailMessage()
        //    {
        //        Subject = subject,
        //        Body = body,
        //    })
        //    {
        //        // Go through distribution list
        //        String[] distList = Properties.distList.Split(',');
        //        foreach (string s in distList)
        //        {
        //            message.To.Add(s);
        //        }

        //        System.Threading.Thread.Sleep(10000);

        //        message.From = fromAddress;
        //        Attachment attachment = new Attachment(Properties.attachmentPath);
        //        message.Attachments.Add(attachment);

        //        try
        //        {
        //            smtp.Send(message);
        //            VescoLog.LogEvent("SMTP Through Attach Gmail Success");

        //        }
        //        catch (Exception ex)
        //        {
        //            Exception ex2 = ex;
        //            string errorMessage = string.Empty;
        //            while (ex2 != null)
        //            {
        //                errorMessage += ex2.ToString();
        //                ex2 = ex2.InnerException;
        //            }
        //            VescoLog.LogEvent("Dude, you're probably plugged in to the VPN!  Shut that shit down!");
        //            VescoLog.LogEvent(ex.StackTrace);
        //            VescoLog.LogEvent(errorMessage);
        //        }
        //    }
        //}

        private static void moveProcessedFile(string origFileName, string origFilePath)
        {
            // Moving the wrong file
            String processedName = origFileName.Insert(origFileName.IndexOf("."), DateTime.Now.ToString("-yyyyMMdd_HHmmssfff"));
            File.Copy(origFilePath, Properties.procDir + processedName); // Try to move
            File.Delete(origFilePath);
            VescoLog.LogEvent("Copied to " + Properties.procDir); // Success
        }

    }
}