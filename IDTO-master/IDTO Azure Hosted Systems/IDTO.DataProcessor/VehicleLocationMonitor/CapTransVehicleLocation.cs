using System;
using System.Collections.Generic;
using System.Linq;
using IDTO.Entity.Models;
using IDTO.Common;
using Repository;
using System.Device.Location;
using IDTO.Common.Storage;
using IDTO.RouteAggregationLibrary.OpenTripPlanner;
using RestSharp;
using System.Threading.Tasks;

namespace IDTO.DataProcessor.VehicleLocationMonitor
{
    public class CapTransVehicleLocation : IVehicleLocation
    {
        const string BaseUrl = "http://otpcride.cloudapp.net/opentripplanner-api-webapp/ws/";
        public Providers ProviderName
        {
            get
            {
                return Providers.CapTrans;
            }
        }
        /// <summary>
        /// List of the geolocations of the two Cap Trans stops
        /// </summary>
        private readonly Dictionary<string, GeoCoordinate> _stops;

      
        /// <summary>
        /// Calculates the distance from the Cap Trans stop destination (one of two possible) to the current
        /// location (from the azure table) and then estimates an arrival time.
        /// </summary>
        /// <param name="tConnect"></param>
        /// <param name="Uow"></param>
        /// <returns></returns>
        public async Task<DateTime> CalculateEstimatedTimeOfArrivalAsync(TConnect tConnect, IUnitOfWork Uow)
        {
            return await Task.Factory.StartNew(() =>
                {
                    GeoCoordinate busStopLoc = GetCoordForInboundStep(tConnect, Uow);

                    LastVehiclePosition lvp = Uow.Repository<LastVehiclePosition>().Query().Get().First(v => v.VehicleName == tConnect.InboundVehicle);
                    DateTime currentLocTime = lvp.PositionTimestamp;
                    GeoCoordinate currentLoc = new GeoCoordinate(lvp.Latitude, lvp.Longitude);

                    DateTime eta = CalcPointBEtaFromPointA(busStopLoc, currentLocTime, currentLoc);
                    return eta;
                });
        }

        /// <summary>
        /// Using the vehicle name in the Tconnect, this function returns the latest known location and time of the vehicle.
        /// </summary>
        /// <param name="tConnect">tconnect containing the vehicle to monitor/check on.</param>
        /// <param name="currentLocTime">returns Time when the latest location was recorded.</param>
        /// <param name="currentLoc">returns latest location of vehicle on record.</param>
        //private void GetLatestVehicleLocation(TConnect tConnect, out DateTime currentLocTime, out GeoCoordinate currentLoc)
        //{
        //    IAzureTable<ProbeSnapshotEntry> probeTable = WorkerRole.Kernel.Get<IAzureTable<ProbeSnapshotEntry>>();
        //    ProbeSnapshotEntry probe = GetLatestLocationFromTable(tConnect.InboundVehicle, probeTable);
        //    //DateTimes in VS by default are 4 hours behind the Date we pull from Azure Tables.
        //    //When we load a snapshot to the Azure table, it is converted to UTC time
        //    //currentLocTime = probe.PositionTimestamp.ToLocalTime();
        //    currentLocTime = probe.PositionTimestamp;
        //    currentLoc = new GeoCoordinate(probe.Latitude, probe.Longitude);
        //}
        /// <summary>
        /// Retrieves the latest ProbeSnapshotEntry for the given inboundVehicle name from the provided AzureTable
        /// </summary>
        /// <param name="inboundVehicle">name of vehicle</param>
        /// <param name="probeTable">Azure Table to check for records.</param>
        /// <returns></returns>
        //protected ProbeSnapshotEntry GetLatestLocationFromTable(string inboundVehicle, IAzureTable<ProbeSnapshotEntry> probeTable)
        //{
        //    //CloudStorageAccount storageAccount = CloudStorageAccount.FromConfigurationSetting("StorageConnectionString");
        //    //IAzureTable<ProbeSnapshotEntry> probeTable = new AzureTable<ProbeSnapshotEntry>(storageAccount);
        //    try
        //    {
        //        List<ProbeSnapshotEntry> tripProbes = probeTable.Query.Where(p => p.PartitionKey == inboundVehicle).ToList();
        //        return tripProbes.OrderBy(p => p.PositionTimestamp).Last();
        //    }
        //    catch(Exception ex)
        //    {
        //        throw new Exception("No ProbeSnapshotEntries found in Azure Table for " + inboundVehicle + ". Exception: " + ex.Message);
        //    }
        //}
        /// <summary>
        /// Based on the location and time at Point A, this function calculates the Estimated Time of Arrival
        /// at another Point, B, based on a fixed rate of 20 mph.
        /// </summary>    
        /// <param name="PointB">destination position</param>
        /// <param name="TimeAtPointA">time at current position</param>
        /// <param name="PointA">current position</param>
        /// <returns></returns>
        protected static DateTime CalcPointBEtaFromPointA(GeoCoordinate PointB, DateTime TimeAtPointA, GeoCoordinate PointA)
        {
            RestClient client = new RestClient
            {
                BaseUrl = BaseUrl
            };
            OpenTripPlannerAdapter otpa = new OpenTripPlannerAdapter(client);

            DateTime eta = new DateTime();

            var trip = otpa.PlanTrip((float)PointA.Latitude, (float)PointA.Longitude, (float)PointB.Latitude, (float)PointB.Longitude, "CAR", DateTime.Now);
            if (trip.plan != null)
            {
                var itinerary = trip.plan.itineraries.FirstOrDefault();
                eta = TimeAtPointA.AddMilliseconds(itinerary.duration);                
            }
            //else we don't have an eta to return
            return eta;

            //Calculate distance between the two points.
            //double distanceMeters = PointB.GetDistanceTo(PointA);//The distance between the two coordinates, in meters.

            //Loosely Estimate an arrival based on an assumed speed. r*t=distance
            //20 mph = 8.9mps
            //15 Miles per Hour = 6.7056 Meters per Second
            //double seconds = distanceMeters / 6.7;
            //DateTime ETA = TimeAtPointA.AddSeconds(seconds);
            //return eta;
        }

        /// <summary>
        /// Finds the Inbound Step involved in this tconnect, and returns the lat/long location of it.
        /// Matches Inbound step to a known list of stops for Cap Trans.
        /// </summary>
        /// <param name="tConnect">tconnect that has the outbound step to resolve to GeoCoordinate.</param>
        /// <param name="Uow"></param>
        /// <returns></returns>
        protected GeoCoordinate GetCoordForInboundStep(TConnect tConnect, IUnitOfWork Uow)
        {
            //Tconnect InboundStepId ToStopCode should match to _stops list. Get correct stop lat long.
            var t = Uow.Repository<TConnect>().Query().Include(i => i.InboundStep).Get()
                        .Where(code => code.Id.Equals(tConnect.Id)).First();
            string busStop = t.InboundStep.ToStopCode;
            if (!_stops.ContainsKey(busStop))
            {
                throw new NotImplementedException("CapTrans bus stop not found: " + busStop);
            }
            GeoCoordinate busStopLoc;
            _stops.TryGetValue(busStop, out busStopLoc);
            return busStopLoc;
        }

        
        /// <summary>
        /// Cap Trans Inbound Vehicle's name will be the name of the traveler, which in turn
        /// is the name of the MDT (MDT1, MDT2, etc)
        /// </summary>
        /// <param name="tConnect"></param>
        /// <param name="Uow"></param>
        /// <returns></returns>
        public string GetInboundVehicleName(TConnect tConnect, IUnitOfWork Uow)
        {
            string vehicleName = "";
            var t = Uow.Repository<TConnect>().Query().Include(i => i.InboundStep.Trip.Traveler).Get()
                          .Where(code => code.Id.Equals(tConnect.Id)).First();
            vehicleName= t.InboundStep.Trip.Traveler.LastName;
            return vehicleName;
        }

        public CapTransVehicleLocation()
        {
            //Stop Codes and GeoCoordinates
            _stops = new Dictionary<string, GeoCoordinate>()
            {
                { "DCSCBRD1", new GeoCoordinate  {Latitude=39.975148, Longitude=-82.894379}},//broad street gate
                { "DCSCJMS1", new GeoCoordinate  {Latitude=39.977592, Longitude=-82.913055}}, //james road gate
                { "BATBLD4", new GeoCoordinate  {Latitude=39.989278, Longitude=-83.018776}} //Battelle Test Shuttle Drop Off
            };

        }
    }
}
