using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.DataProcessor.Common;
using IDTO.Common;
using Ninject;
using Repository;
using IDTO.Entity.Models;
using IDTO.DataProcessor.VehicleLocationMonitor;
using Repository.Providers.EntityFramework;
using IDTO.Data;
using Microsoft.WindowsAzure.ServiceRuntime;
using SendGrid;
using SendGrid.SmtpApi;
using System.Net;
using System.Net.Mail;

namespace IDTO.DataProcessor.TravelerMonitor
{
    public class TravelerMonitorWorker : BaseProcWorker
    {
        protected IDbContext db;//= new IDTOContext();
        public IUnitOfWork Uow;

        protected static string SENDGRID_USERNAME = "azure_ebd97354501a43f2f9d293a272665645@azure.com";
        protected static string SENDGRID_PASSWORD = "341S3yHEKdT5d7R";


        public TravelerMonitorWorker()
        {
        }

        /// <summary>
        /// If this process modifies a database record that requires a "modified by", this is what we'll fill in.
        /// </summary>
        private string ModifiedBy = "TravelerMonitorWorker";
        public override void PerformWork()
        {
            // Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral, DateTime.UtcNow.ToLongTimeString() + " - Entering PerformWork for TConnectMonitorWorker");
            IDbContext db = WorkerRole.Kernel.Get<IDbContext>();

            Uow = new UnitOfWork(db);
            //SendInactivityEmails(Uow);
            SendEmailsAtEndOfTrip(Uow);
        }

        public void SendEmailsAtEndOfTrip(IUnitOfWork Uow)
        {
            List<Traveler> travelers = Uow.Repository<Traveler>().Query().Get().ToList();

            DateTime dtNow = DateTime.UtcNow;

            foreach (Traveler t in travelers)
            {
                DateTime lastContactDate;

                if (t.LastContactedDate.HasValue)
                    lastContactDate = t.LastContactedDate.Value;
                else
                    lastContactDate = DateTime.MinValue;

                List<Trip> trips = Uow.Repository<Trip>().Query().Get()
                    .Where(s => s.TravelerId == t.Id && s.TripEndDate > lastContactDate && s.TripEndDate < dtNow).OrderByDescending(s => s.CreatedDate).ToList();

                int tripCount = trips.Count;

                if (tripCount >= 2)
                {
                    // send email

                    string userIdString = t.Id.ToString();

                    string emailBody = "<p>Dear " + t.FirstName + ",</p>" +
                    "<p>Thank you for using the C-Ride application for your public transportation needs. It’s important to us that we make your travel experiences easy and accessible through the use of C-Ride.  We noticed that you used the app recently and would like feedback around your experience with C-Ride.  Please take time to fill out the short survey by clicking on the link below.</p>" +
                    "<p><a href=\"https://www.surveymonkey.com/s/N57Z23L?user_id="+t.Id.ToString()+"\">C-Ride User Experience Survey</a></p>" +
                    "<p>Thank You,<br/>" +
                    "The C-Ride Team</p>";


                    sendEmail(t.Email,
                        "Connect and Ride User Experience Survey",
                        emailBody);

                    t.LastContactedDate = DateTime.UtcNow;

                    Uow.Repository<Traveler>().Update(t);
                    Uow.Save();
                }

            }
        }

        public void SendInactivityEmails(IUnitOfWork Uow)
        {
            List<Traveler> travelers = Uow.Repository<Traveler>().Query().Get().ToList();

            DateTime dtNow = DateTime.UtcNow;

            foreach (Traveler t in travelers)
            {
                List<Trip> trips = Uow.Repository<Trip>().Query().Get().Where(s => s.TravelerId == t.Id).OrderByDescending(s => s.CreatedDate).ToList();
                Trip firstTrip = trips.FirstOrDefault();

                if (Math.Abs((dtNow - firstTrip.CreatedDate).TotalDays) > 30)
                {
                    double totalDays = 0;
                    if (t.LastContactedDate != null)
                        totalDays = Math.Abs((dtNow - t.LastContactedDate.Value).TotalDays);

                    if (t.LastContactedDate == null || totalDays > 30)
                    {
                        // send email

                        string userIdString = t.Id.ToString();

                        string emailBody = "<p>Dear " + t.FirstName + ",</p>" +
                        "<p>Thank you for downloading the C-Ride application. It’s important to us that we make your travel experiences around Columbus easy and accessible through the use of C-Ride." +
                        "  We noticed that you haven’t used the app recently and would like to ensure it’s as easy as possible for you to find, plan and schedule trips. We’ve put together a few tips to help explain ways to maximize C-Ride <a href=\"www.connectandride.com/Home/UserTips\">C-Ride User Tips.</a>" +
                        "  If you have feedback around your application experience in general, we’ve included a short survey for you to share your thoughts.</p>" +
                        "<p><a href=\"https://www.surveymonkey.com/s/DRFRJMD?user_id=" + userIdString + "\">C-Ride Customer Survey</a></p>" +
                        "<p>Thank You,<br/>" +
                        "The C-Ride Team</p>";



                        sendEmail(t.Email,
                            "Come Back to Connect and Ride",
                            emailBody);

                        t.LastContactedDate = DateTime.UtcNow;

                        Uow.Repository<Traveler>().Update(t);
                        Uow.Save();
                    }
                }
                
            }
        }

        private void sendEmail(String emailAddress, string subject, string html)
        {
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(emailAddress);
            myMessage.From = new MailAddress("connectandride@battelle.org", "Connect and Ride");
            myMessage.Subject = subject;
            myMessage.Html = html;

            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(SENDGRID_USERNAME, SENDGRID_PASSWORD);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);
            transportWeb.Deliver(myMessage);
        }

        public void SendTConnectEmail(IUnitOfWork Uow)
        {
            List<TConnect> tconnects = Uow.Repository<TConnect>().Query().Include(s=>s.OutboundStep.Trip.Traveler).Get()
                .Where(s => s.SurveyDate == null).ToList();


            foreach(TConnect t in tconnects)
            {
                int tripId = t.OutboundStep.TripId;

                string messageHtml = "<p>Greetings.  We see that you have recently traveled using one or more of the transportation providers that support the Connect and Ride application.  We hope your trip was successful and on schedule!  If you recall when you registered to use C-Ride, the agreement indicated that C-Ride was created in order to support research related to people’s travel experience using public and alternative forms of transportation. As a user of these forms of transportation, we would ask for you to participate in a brief online survey.  The link below will take you directly to this survey.  Your honest feedback is important to understanding how and where to make future investments related to these forms of transportation.</p>"
                    + "<a href=http://battelle.surveymonkey.com/survey.asp%ID=" + tripId.ToString() + ">Post Trip Survey</a>"
                    + "<p>Sincerely,</p>"
                    + "<p>The Connect and Ride Research Team</p>";

                sendEmail(t.OutboundStep.Trip.Traveler.Email, "Connect And Ride Post Trip Survey", messageHtml);
            }


        }
    }
}
