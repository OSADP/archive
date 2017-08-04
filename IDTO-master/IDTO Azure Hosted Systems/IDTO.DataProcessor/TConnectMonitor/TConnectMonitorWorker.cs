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
using IDTO.BusScheduleInterface;

namespace IDTO.DataProcessor.TConnectMonitor
{
    public class TConnectMonitorWorker : BaseProcWorker
    {
        private Dictionary<int, LocationMonitoringNotificationSwitchboard> locationNotificationTrackingDictionary
            = new Dictionary<int, LocationMonitoringNotificationSwitchboard>();

        protected IDbContext db;//= new IDTOContext();
        public IUnitOfWork Uow;
        private PushNotificationManager notificationManager;
        private List<IBusSchedule> busSchedulerAPIs;

        public TConnectMonitorWorker(List<IBusSchedule> busSchedulerAPIs)
        {
            String EndPointConnection = RoleEnvironment.GetConfigurationSettingValue("EndPointConnection");
            String HubName = RoleEnvironment.GetConfigurationSettingValue("HubName");

            notificationManager = new PushNotificationManager(EndPointConnection,HubName);

            this.busSchedulerAPIs = busSchedulerAPIs;
        }

        /// <summary>
        /// If this process modifies a database record that requires a "modified by", this is what we'll fill in.
        /// </summary>
        private string ModifiedBy = "TConnectMonitor";
        public async override void PerformWork()
        {
            // Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral, DateTime.UtcNow.ToLongTimeString() + " - Entering PerformWork for TConnectMonitorWorker");
            IDbContext db = WorkerRole.Kernel.Get<IDbContext>();

            Uow = new UnitOfWork(db);
       
            SetupNewTConnects(Uow);
            await MonitorTConnects(Uow);
            SendTripStartNotifications(Uow);
        }

        /// <summary>
        /// Check all TConnects with status of "Monitored" to see if they need a TConnectRequest issued.
        /// </summary>
        /// <param name="Uow"></param>
        private async Task MonitorTConnects(IUnitOfWork Uow)
        {
            List<TConnect> monitoredTConnects = Uow.Repository<TConnect>().Query().Include(s => s.OutboundStep).Include(s => s.InboundStep.Trip).Get()
                       .Where(s => s.TConnectStatusId == (int)TConnectStatuses.Monitored).ToList();

            foreach (TConnect mt in monitoredTConnects)
            {   
                try
                {
                    try
                    {
                        await MonitorLocationTimes(mt);
                        MonitorTravelerLocation(mt);
                    }catch(Exception ex2)
                    {
                        String innerExString = "";
                        String stackTraceString = "";
                        if (ex2.InnerException != null)
                        {
                            innerExString = ex2.InnerException.Message;

                            if (ex2.InnerException.StackTrace != null)
                                stackTraceString = ex2.InnerException.StackTrace.ToString();
                        }

                        Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Error, TraceEventId.TraceGeneral, " Error in MonitorLocationTimes for TConnect: " + mt.Id + ".  " + ex2.Message + " InnerEx: " + innerExString + " StackTrace: " + stackTraceString);
                    }

                    if (DateTime.UtcNow > mt.EndWindow)
                    {
                        //remove the tracking of notifications when monitoring is done
                        locationNotificationTrackingDictionary.Remove(mt.InboundStep.TripId);

                        Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral, "TConnect Window Done.  tConnect.Id=" + mt.Id);  
                        //The time has gone past the latest time we could possible issue a Request for this.
                        //Set it to Done so we stop checking.
                        mt.TConnectStatusId = (int)TConnectStatuses.Done;
                        Uow.Repository<TConnect>().Update(mt);
                        Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Done monitoring T-Connect.  Current time outside End Window"));
                        Uow.Save();
                    }
                    else if (mt.InboundStep.StartDate > DateTime.UtcNow)
                    {
                        //leg to monitor has not started yet. nothing to do.
                        Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral, "TConnect not yet started.  tConnect.Id=" + mt.Id);  
                    }
                    else
                    {

                        Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral, "Monitored TConnect.  tConnect.Id=" + mt.Id);  
                        
                        DateTime newEta = await GetEtaForTConnectAsync(Uow, mt, busSchedulerAPIs);
                        if (newEta.Year > new DateTime().Year)
                        {
                            //Check to make sure we have not generally "arrived" by comparing the time the probe was posted to the eta calculated.
                            LastVehiclePosition lvp = Uow.Repository<LastVehiclePosition>().Query().Get()
                                .FirstOrDefault(v => v.VehicleName == mt.InboundVehicle);
                            if (lvp !=null && newEta - lvp.PositionTimestamp <= System.TimeSpan.FromMinutes(1))
                            {
                                //We are there. Time to stop monitoring.
                                mt.TConnectStatusId = (int)TConnectStatuses.Done;
                                Uow.Repository<TConnect>().Update(mt);
                                Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Done monitoring T-Connect.  Bus has arrived."));
                                Uow.Save();
                            }
                            else
                            {
                                MonitorTConnect(Uow, mt, newEta);
                            }
                        }
                        else
                        {
                            Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Unable to calculate ETA"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    String innerExString = "";
                    String stackTraceString = "";
                    if (ex.InnerException != null)
                    {
                        innerExString = ex.InnerException.Message;

                        if (ex.InnerException.StackTrace != null)
                            stackTraceString = ex.InnerException.StackTrace.ToString();
                    }

                    Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Error, TraceEventId.TraceGeneral, " Error in MonitorTConnects for TConnect: " + mt.Id + ".  " + ex.Message + " InnerEx: " + innerExString + " StackTrace: " + stackTraceString);
                }
            }

        }

        private void MonitorTravelerLocation(TConnect mt)
        {
            try
            {
                if (DateTime.UtcNow >= mt.InboundStep.StartDate && DateTime.Now <= mt.InboundStep.StartDate.AddMinutes(1))
                {
                    //get traveler location
                    int travelerId = mt.InboundStep.Trip.TravelerId;

                    var travelerLocations = Uow.Repository<TravelerLocation>().Query()
                        .Get().Where(s => s.TravelerId == travelerId).OrderByDescending(s => s.PositionTimestamp);

                    var lastTravelerLocation = travelerLocations.FirstOrDefault<TravelerLocation>();

                    if (lastTravelerLocation != null)
                    {
                        //get vehicle location nearest to the traveler location time
                        LastVehiclePosition lvp = Uow.Repository<LastVehiclePosition>().Query().Get().First(v => v.VehicleName == mt.InboundVehicle);

                        //get distance between them

                        double dist_m = Conversions.distanceMeters(lastTravelerLocation.Latitude, lastTravelerLocation.Longitude,
                            lvp.Latitude, lvp.Longitude);

                        //get time between the vehilcle location and the traveler location
                        TimeSpan tsDiff = lastTravelerLocation.PositionTimestamp - lvp.PositionTimestamp;


                        //do a fuzzy match, like distance within 100 meters
                        if (Math.Abs(tsDiff.TotalSeconds) < 30)
                        {
                            if (dist_m > 100)
                            {
                                //We are there. Time to stop monitoring.
                                mt.TConnectStatusId = (int)TConnectStatuses.Done;
                                Uow.Repository<TConnect>().Update(mt);
                                Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Done monitoring T-Connect." +
                                " Traveler never arrived and is " + dist_m.ToString() + " meters away." +
                                " Traveler loc (" + lastTravelerLocation.Latitude.ToString() + ","
                                + lastTravelerLocation.Longitude.ToString() + ") @ " +
                                lastTravelerLocation.PositionTimestamp.ToString("hh:mm:ss") +
                                " Vehicle loc (" + lvp.Latitude.ToString() + ","
                                + lvp.Longitude.ToString() + ") @ " +
                                lvp.PositionTimestamp.ToString("hh:mm:ss")
                                ));
                                Uow.Save();
                            }
                        }
                    }
                }
            }catch(Exception ex)
            {
                String innerExString = "";
                String stackTraceString = "";
                if (ex.InnerException != null)
                {
                    innerExString = ex.InnerException.Message;

                    if (ex.InnerException.StackTrace != null)
                        stackTraceString = ex.InnerException.StackTrace.ToString();
                }

                Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Error, TraceEventId.TraceGeneral, " Error in MonitorTravelerLocation for TConnect: " + mt.Id + ".  " + ex.Message + " InnerEx: " + innerExString + " StackTrace: " + stackTraceString);
            }
        }

        private async Task MonitorLocationTimes(TConnect mt)
        {
            TimeSpan ts = TimeSpan.FromSeconds(120);

            LocationMonitoringNotificationSwitchboard switchboard = new LocationMonitoringNotificationSwitchboard();
            if(locationNotificationTrackingDictionary.ContainsKey(mt.InboundStep.TripId))
                switchboard = locationNotificationTrackingDictionary[mt.InboundStep.TripId];

            Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                "MonitorLocationTimes  tConnect.Id=" + mt.Id + "InboundStartDate="+mt.InboundStep.StartDate.ToLongTimeString()+
                "NowUtc="+DateTime.UtcNow.ToLongTimeString() + "sent=" + switchboard.SentStartInboundStep.ToString());  

            if (DateTime.UtcNow >= mt.InboundStep.StartDate.AddMinutes(-1) && switchboard.SentStartInboundStep==false)
            {
                Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                    "MonitorLocationTimes Sending Start Monitoring Locationat Start of Inbound Step for  tConnect.Id=" + mt.Id);  
                //send notification to track location
                bool result = await notificationManager.SendIOSSilentNotificationAsync(
                       mt.InboundStep.Trip.TravelerId.ToString(), (int)ts.TotalSeconds);

                result = await notificationManager.SendGcmSilentNotificationAsync(
                    mt.InboundStep.Trip.TravelerId.ToString(), (int)ts.TotalSeconds);
                Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Send notification to start monitoring start of inbound step location for " + ts.TotalSeconds.ToString() + " seconds"));
                Uow.Save();
                switchboard.SentStartInboundStep = true;
            }

            Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                "MonitorLocationTimes  tConnect.Id=" + mt.Id + "InboundEndDate=" + mt.InboundStep.EndDate.ToLongTimeString() +
                "NowUtc=" + DateTime.UtcNow.ToLongTimeString() + "sent=" + switchboard.SentEndInboundStep.ToString());  

            if (DateTime.UtcNow >= mt.InboundStep.EndDate.AddMinutes(-1) && switchboard.SentEndInboundStep == false)
            {
                Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                    "MonitorLocationTimes Sending Start Monitoring Locationat End of Inbound Step for  tConnect.Id=" + mt.Id);  
                //send notification to track location
                bool result = await notificationManager.SendIOSSilentNotificationAsync(
                       mt.InboundStep.Trip.TravelerId.ToString(), (int)ts.TotalSeconds);

                result = await notificationManager.SendGcmSilentNotificationAsync(
                    mt.InboundStep.Trip.TravelerId.ToString(), (int)ts.TotalSeconds);
                Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Send notification to start monitoring end of inbound step location for " + ts.TotalSeconds.ToString() + " seconds"));
                Uow.Save();
                switchboard.SentEndInboundStep = true;
            }

            Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                "MonitorLocationTimes  tConnect.Id=" + mt.Id + "OutboundStartDate=" + mt.OutboundStep.StartDate.ToLongTimeString() +
                "NowUtc=" + DateTime.UtcNow.ToLongTimeString() + "sent=" + switchboard.SentStartOutboundStep.ToString());  
            if (DateTime.UtcNow >= mt.OutboundStep.StartDate.AddMinutes(-1) && switchboard.SentStartOutboundStep == false)
            {
                Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                    "MonitorLocationTimes Sending Start Monitoring Locationat Start of Outbound Step for  tConnect.Id=" + mt.Id);  
                //send notification to track location
                bool result = await notificationManager.SendIOSSilentNotificationAsync(
                       mt.InboundStep.Trip.TravelerId.ToString(), (int)ts.TotalSeconds);

                result = await notificationManager.SendGcmSilentNotificationAsync(
                    mt.InboundStep.Trip.TravelerId.ToString(), (int)ts.TotalSeconds);
                Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Send notification to start monitoring start of outbound step location for " + ts.TotalSeconds.ToString() + " seconds"));
                Uow.Save();
                switchboard.SentStartOutboundStep = true;

            }

            if (locationNotificationTrackingDictionary.ContainsKey(mt.InboundStep.TripId))
                locationNotificationTrackingDictionary[mt.InboundStep.TripId] = switchboard;
            else
                locationNotificationTrackingDictionary.Add(mt.InboundStep.TripId, switchboard);

        }

        /// <summary>
        /// Checks TConnect to see if TConnectRequest already exists for this TConnect.
        /// Updates TConnectRequest (if already exists) or creates a new one if needed.
        /// </summary>
        /// <param name="Uow"></param>
        /// <param name="mt"></param>
        /// <param name="newEta">Expected arrival of the inbound vehicle for the tConnect</param>
        protected void MonitorTConnect(IUnitOfWork Uow, TConnect mt, DateTime newEta)
        {           
            //First determine if we are updating an item that already has an active TRequest, or if
            //this is a general Monitored vehicle that has not yet generated a TConnectRequest.

            //Check TConnectRequest table for a matching row for this tconnect
            var tr = Uow.Repository<TConnectRequest>().Query().Get().Where(s => s.TConnectId.Equals(mt.Id));

            //Will need outbound departure time and stopcode
            Step outboundStep = Uow.Repository<Step>().Query().Include(s=>s.Block).Get().First(s => s.Id.Equals(mt.OutboundStepId));

            if (tr.Any())
            {
                TConnectRequest treq = tr.First();
                UpdateExistingTConnectRequest(Uow, mt, newEta, treq);
            }
            else
            {
                CreateNewTConnectRequestIfNeeded(Uow, mt, newEta, outboundStep);
            }
        }
        /// <summary>
        /// Checks the Eta for the inbound vehicle to the departure window that would warrant
        /// a TConnectRequest.  Creates a TConnectRequest if possible.
        /// </summary>
        /// <param name="Uow"></param>
        /// <param name="tConnect"></param>
        /// <param name="newEta"></param>
        /// <param name="outboundStep"></param>
        async protected void CreateNewTConnectRequestIfNeeded(IUnitOfWork Uow, TConnect tConnect, DateTime newEta, Step outboundStep)
        {
           
            if (newEta > tConnect.StartWindow && newEta <= tConnect.EndWindow)
            {
                //We want to issue a TConnect Request.
                Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                      "Requesting TConnect. newEta=" + newEta.ToShortTimeString()
                      + "  endwindow=" + tConnect.EndWindow.ToString() + " tConnect.Id=" + tConnect.Id);

                TConnectRequest newRequest = new TConnectRequest();

                //Need a TConnectedVehicle for this new request.
                int acceptedWaitTime = 0;
                newRequest.TConnectedVehicleId = GetOrCreateTConnectedVehicleKey(Uow, outboundStep, out acceptedWaitTime);//set ref to vehicle
                newRequest.TConnectId = tConnect.Id;//set ref to owning tConnect.
                newRequest.EstimatedTimeArrival = newEta;
                //Calculate how late we'll be
                newRequest.RequestedHoldMinutes = (int)System.Math.Ceiling((newEta - (DateTime)tConnect.StartWindow).TotalMinutes);
                newRequest.ModifiedBy = ModifiedBy;
                newRequest.ModifiedDate = DateTime.UtcNow;
                if (newRequest.RequestedHoldMinutes <= acceptedWaitTime)
                {
                    //This request is asking for a wait less than the amount the bus already agreed to, so this
                    //is automatically accepted.
                    newRequest.TConnectStatusId = (int)TConnectStatuses.Accepted;
                    Uow.Repository<TripEvent>().Insert(new TripEvent(outboundStep.TripId, "T-Connect Request created and auto accapted"));
                }
                else
                {
                    newRequest.TConnectStatusId = (int)TConnectStatuses.New;//new request  
                    Uow.Repository<TripEvent>().Insert(new TripEvent(outboundStep.TripId, "T-Connect Request created."));
                }     
               
                Uow.Repository<TConnectRequest>().Insert(newRequest);                
                Uow.Save();

            }
            else if (newEta > tConnect.EndWindow)
            {
                //Will be Too late to issue a TConnect, bus wont wait this long.

                Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Information, TraceEventId.TraceGeneral,
                    "Bus Missed before there was a chance to issue a request. newEta=" + newEta.ToShortTimeString()
                    + "  endwindow=" + tConnect.EndWindow.ToString() + " tConnect.Id=" + tConnect.Id);

                //Update status to done.
                tConnect.TConnectStatusId = (int)TConnectStatuses.Done;
                Uow.Repository<TConnect>().Update(tConnect);
                Uow.Repository<TripEvent>().Insert(new TripEvent(outboundStep.TripId, "T-Connect Request not created, Inbound bus will be too late."));
                Uow.Save();

                //Notify Traveler of TConnect creation
                //TODO need to find traveller device type (Android, IOS, etc) for now defaulting to Android
                //find traveler email and send push update
                
                Traveler t = Uow.Repository<Traveler>().Query().Get().Where(s => s.Id == tConnect.InboundStep.Trip.TravelerId).First();
                if (t != null && t.Email != null)
                {
                    bool result = await notificationManager.SendRejectNotificationsAsync(t.Email);
                }
            }
            else
            {
                string msg = "No need to issue a request yet. Eta=" + newEta.ToShortTimeString() + " : ";
                msg += "T-Connect Window [" + ((DateTime) tConnect.StartWindow).ToShortTimeString() + "-";
                msg += ((DateTime)tConnect.EndWindow).ToShortTimeString() + "]";
                LogTripEventIfDifferentFromLast(Uow, outboundStep.TripId, msg);
                //really verbose message
                Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral,
                   "No need to issue a request yet. newEta=" + newEta.ToShortTimeString()
                   + "  endwindow=" + tConnect.EndWindow.ToString() + " tConnect.Id=" + tConnect.Id);
            }
        }

        /// <summary>
        /// Logs the trip event if different from last message recorded for this particular trip
        /// </summary>
        /// <param name="Uow">The uow.</param>
        /// <param name="tripId">The trip identifier.</param>
        /// <param name="message">The message to be logged</param>
        private static void LogTripEventIfDifferentFromLast(IUnitOfWork Uow, int tripId, string message)
        {
            try
            {
                TripEvent lastTripEvent = Uow.Repository<TripEvent>()
                    .Query().Get().Where(te => te.TripId == tripId)
                    .OrderByDescending(te => te.EventDate)
                    .FirstOrDefault();

                if (lastTripEvent != null && !lastTripEvent.Message.Equals(message))
                {
                    Uow.Repository<TripEvent>().Insert(new TripEvent(tripId, message));
                    Uow.Save();
                }
            }
            catch (Exception ex)
            {
                //Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Error, TraceEventId.TraceGeneral, " Error in LogTripEventIfDifferentFromLast for Trip: " + tripId + ".  " + ex.Message);
            }
        }

        /// <summary>
        /// Another TConnectRequest may already tconnect to the same vehicle as this one, if so,
        /// returns the key id to that existing row. Otherwise, creates a new TConnectedVehicle entry
        /// in the database and returns that key
        /// </summary>
        /// <param name="Uow"></param>
        /// <param name="outboundStep"></param>
        /// <param name="acceptedHoldMinutes">The currently agreed upon number of minutes the bus will wait after
        /// the scheduled departure.  For a new TConnectedVehicle, this is always zero, but for one that was
        /// created prior, it may already have an accepted wait time.</param>
        /// <returns></returns>
        private int GetOrCreateTConnectedVehicleKey(IUnitOfWork Uow, Step outboundStep, out int acceptedHoldMinutes)
        {
            //See if one already exists -- look for matching stop code and departure time
            var tveh = Uow.Repository<TConnectedVehicle>().Query().Get().Where(s => s.TConnectStopCode.Equals(outboundStep.FromStopCode)
                && s.OriginallyScheduledDeparture.Equals(outboundStep.StartDate));

            if (tveh.Count() >0)
            {
                TConnectedVehicle tv = tveh.First();
                //There is already another tconnectrequest issued to this vehicle
                acceptedHoldMinutes = tv.CurrentAcceptedHoldMinutes;
                return( tv.Id);
            }
            else
            {
                //This vehicle does not have any tconnectrequests yet, we have to make a row in
                //the vehicle table.
                TConnectedVehicle newTv = new TConnectedVehicle
                {
                    CurrentAcceptedHoldMinutes = acceptedHoldMinutes = 0,
                    OriginallyScheduledDeparture = outboundStep.StartDate,
                    TConnectStopCode = outboundStep.FromStopCode,
                    TConnectRoute = outboundStep.RouteNumber,
                    TConnectFromName=outboundStep.FromName,
                    TConnectBlockIdentifier=outboundStep.BlockIdentifier,
                    ModifiedBy = ModifiedBy,
                    ModifiedDate = DateTime.UtcNow
                };
                Uow.Repository<TConnectedVehicle>().Insert(newTv);
                Uow.Save();
                //Save newly created foreign key for new tconnectrequest
                return( newTv.Id);
            }
        }

        /// <summary>
        /// A TConnectRequest had previously been created for this TConnect.  Update it
        /// with new ETA information if the inbound vehicle is falling further behind schedule.
        /// The update will revert the TConnectRequest to status New, unless another TConnectRequest already
        /// has been accepted with a longer time.
        /// </summary>
        /// <param name="Uow"></param>
        /// <param name="newEta"></param>
        /// <param name="tr"></param>
        /// <param name="outboundStep"></param>
        async private static void UpdateExistingTConnectRequest(IUnitOfWork Uow, TConnect tConnect, DateTime newEta, TConnectRequest tr)
        {
            string msg = "Still monitoring TConnect. Eta=" + newEta.ToShortTimeString() + " : ";
            msg += "T-Connect Window [" + ((DateTime)tConnect.StartWindow).ToShortTimeString() + "-";
            msg += ((DateTime)tConnect.EndWindow).ToShortTimeString() + "]";
            LogTripEventIfDifferentFromLast(Uow, tConnect.InboundStep.TripId, msg);
           
            //We've already created a tConnect for this item, lets update it to see
            //if it has gotten even later.
            int newHoldMinutes = (int)Math.Ceiling((newEta - (DateTime)tConnect.StartWindow).TotalMinutes);

            if (newHoldMinutes > tr.RequestedHoldMinutes)//inbound getting even later.
            {
                tr.EstimatedTimeArrival = newEta;
                tr.RequestedHoldMinutes = newHoldMinutes;
                //By default, this updated time will be placed as "new" since we don't know if 
                //this alteration is acceptable.
                //tr.TConnectStatusId = (int)TConnectStatuses.New;

                //However, If the ETA is beyond the EndWindow, then we know the bus will not
                //wait this long , and it must be automatically rejected.
                if (newEta > tConnect.EndWindow)
                {
                    tr.TConnectStatusId = (int)TConnectStatuses.Rejected;
                    //End monitoring --- Update status to done.
                    tConnect.TConnectStatusId = (int)TConnectStatuses.Done;
                    Uow.Repository<TConnect>().Update(tConnect);
                    Uow.Repository<TripEvent>().Insert(new TripEvent(tConnect.InboundStep.TripId, "ETA of " + newEta.ToShortTimeString() + " is outside the T-Connect End Window"));
                }

                //Last, see if anyone else has requested a wait this long and had it accepted.
                //If so, we know this is accepted too.
                var l = Uow.Repository<TConnectedVehicle>().Query().Get().Where(s => s.Id.Equals(tr.TConnectedVehicleId));

                if (l.Any())
                {
                    int acceptedWaitTime = l.First().CurrentAcceptedHoldMinutes;
                    if (acceptedWaitTime >= newHoldMinutes && tr.TConnectStatusId != (int)TConnectStatuses.Accepted)
                    {
                        //A longer time has been accepted, so we can accept this too.
                        tr.TConnectStatusId = (int)TConnectStatuses.Accepted;
                        Uow.Repository<TripEvent>().Insert(new TripEvent(tConnect.InboundStep.TripId, "T-Connect Request set to Accepted due to previous accepted request"));
                        Uow.Repository<TConnectRequest>().Update(tr);
                    }
                }
            }
            else
            {
                //update arrival time, even though it is running earlier
                //and not at risk for missing connection
                tr.EstimatedTimeArrival = newEta;
                tr.RequestedHoldMinutes = newHoldMinutes;

                Uow.Repository<TConnectRequest>().Update(tr);
            }

            Uow.Save(); 
        }

        /// <summary>
        /// Calculates eta for the inbound vehicle involved in this TConnect
        /// </summary>
        /// <param name="Uow"></param>
        /// <param name="mt"></param>
        /// <returns></returns>
        private static async Task<DateTime> GetEtaForTConnectAsync(IUnitOfWork Uow, TConnect mt, List<IBusSchedule> busSchedulerAPIs)
        {
            IVehicleLocation vehLoc;
            vehLoc = VehicleLocationMonitorWorker.ResolveVehicleLocationProviderType(mt, Uow, busSchedulerAPIs);
            DateTime newEta = await vehLoc.CalculateEstimatedTimeOfArrivalAsync(mt, Uow);
            return newEta;
        }

        /// <summary>
        /// For each newly added TConnect added via the Trip Service, resolve the inbound vehicle name
        /// and update the status for the record.
        /// </summary>
        /// <param name="Uow"></param>
        async public void SetupNewTConnects(IUnitOfWork Uow)
        {
            List<TConnect> monitoredTConnects = Uow.Repository<TConnect>().Query().Include(s => s.InboundStep.Trip).Include(s=>s.OutboundStep).Get()
                     .Where(m => m.TConnectStatusId == (int)TConnectStatuses.New).ToList();

            foreach (TConnect mt in monitoredTConnects)
            {
                try
                {
                    //T-Connect Monitor should not start monitoring the T-Connect until 1 minute before the start of the Trip
                    if (DateTime.UtcNow >= mt.InboundStep.Trip.TripStartDate.AddMinutes(-1) )
                    {
                        //Update Tconnect created with Status New.
                        //InboundVehicle,StartWindow,EndWindow all need updated.
                        IVehicleLocation veh = VehicleLocationMonitorWorker.ResolveVehicleLocationProviderType(mt, Uow, busSchedulerAPIs);
                        mt.InboundVehicle = veh.GetInboundVehicleName(mt, Uow);

                        //Step step = Uow.Repository<Step>().Query().Get()
                        //     .Where(s => s.Id.Equals(mt.OutboundStepId)).First();
                        //Provider prov = step.FromProvider;

                        mt.ModifiedDate = DateTime.UtcNow;
                        mt.ModifiedBy = ModifiedBy;
                        //Update status to monitored now that it has been setup
                        mt.TConnectStatusId = (int)TConnectStatuses.Monitored;
                        Uow.Repository<TConnect>().Update(mt);
                        Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "Identified Inbound Vehicle for T-Connect"));
                        Uow.Repository<TripEvent>().Insert(new TripEvent(mt.InboundStep.TripId, "T-Connect status changed to Monitored"));
                        Uow.Save();


                    }
                }
                catch (Exception ex)
                {
                    Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Error, TraceEventId.TraceGeneral, " Error in SetupNewTConnects for TConnect: " + mt.Id + ".  " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Called every TConnectMonitorWorker cycle; pulls a list of unnotified trips that are ~5 minutes before trip start time.
        /// Sends notification to the traveler's smart device and updates the trip record that the notification has been sent to avoid repeated notifications
        /// </summary>
        /// <param name="Uow"></param>
        async protected void SendTripStartNotifications(IUnitOfWork Uow)
        {
            try
            {
                List<Trip> newTrips = Uow.Repository<Trip>().Query().Include(s => s.Traveler).Get().Where(t => t.TripStartNotificationSent == false).ToList();

                

                foreach (Trip nt in newTrips)
                {
                    if (DateTime.UtcNow >= nt.TripStartDate.AddMinutes(-5) && nt.TripStartDate >= DateTime.UtcNow)
                    {
                        try
                        {

                            //Sending both notification types as we do not know what type of device the user is activly using
                            bool result = await notificationManager.SendIOSTripStartNotificationAsync(nt.Traveler.Email, "Trip to : " + nt.Destination + " is about to start", nt.Id.ToString());

                            result = await notificationManager.SendGcmTripStartNotificationAsync(nt.Traveler.Email, "Trip to : " + nt.Destination + " is about to start", nt.Id.ToString());
                            Uow.Repository<TripEvent>().Insert(new TripEvent(nt.Id, "Trip start notification sent"));
                            nt.TripStartNotificationSent = true;
                            Uow.Repository<Trip>().Update(nt);
                            Uow.Save();
                        }
                        catch (Exception ex)
                        {
                            Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Error, TraceEventId.TraceGeneral, " Error in SendTripStartNotifications for Trip: " + nt.Id + ".  " + ex.Message);
                        }
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
