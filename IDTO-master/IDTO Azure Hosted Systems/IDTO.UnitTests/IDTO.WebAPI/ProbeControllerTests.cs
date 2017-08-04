using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Moq;
using NUnit.Framework;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using IDTO.Common;
using IDTO.Common.Storage;
using Microsoft.WindowsAzure;
using Repository;
//using Microsoft.WindowsAzure.ServiceRuntime;

namespace IDTO.UnitTests.IDTO.WebAPI
{
    [TestFixture]
    public class ProbeControllerTests
    {
        private Trip trip;
        private static void SetupControllerForTests(ApiController controller)
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/PostProbeData");
            var route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
            var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "PostProbeData" } });

            controller.ControllerContext = new HttpControllerContext(config, routeData, request);
            controller.Request = request;
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

            
        }

        private void AddTripToRepo(IUnitOfWork imuow)
        {
            this.trip = new Trip
            {
                TravelerId = 1,
                Origination = "Neverland",
                Destination = "Montana",
                TripStartDate = DateTime.Parse("1/2/2014"),
                TripEndDate = DateTime.Parse("1/3/2014"),
                MobilityFlag = true,
                BicycleFlag = true,
                PriorityCode = "1"
            };
            //add a trip
            imuow.Repository<Trip>().Insert(trip);
            imuow.Save();
        }

        [Test]
        public void PostProbeData_ValidProbeVehicleData_Success()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork imuow = new UnitOfWork(idtoFakeContext))
            {
                AddTripToRepo(imuow);

                DateTime newestPositionTimestamp = DateTime.UtcNow.AddMinutes(1);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long newestTimeStamp = Convert.ToInt64((newestPositionTimestamp - epoch).TotalMilliseconds);

                ProbeVehicleData probeData = new ProbeVehicleData { InboundVehicle = "MDT2" };
                PositionSnapshot ps = new PositionSnapshot()
                {
                    Accuracy = 5,
                    Altitude = 123,
                    Heading = 180,
                    Latitude = 44.646581369493,
                    Longitude = -96.6830267664,
                    Satellites = 0,
                    Speed = 14.77999305725097,
                    TimeStamp = newestTimeStamp
                };
                probeData.Positions.Add(ps);

                var mockTable = new Mock<IAzureTable<ProbeSnapshotEntry>>();
                var cut = new ProbeController(idtoFakeContext, mockTable.Object);

                SetupControllerForTests(cut);

                HttpResponseMessage returnMessage = cut.PostProbeData(probeData);

                Assert.AreEqual(HttpStatusCode.NoContent, returnMessage.StatusCode);

                List<LastVehiclePosition> lvpList = imuow.Repository<LastVehiclePosition>().Query().Get().Where(v => v.VehicleName == "MDT2").ToList();

                Assert.AreEqual(1, lvpList.Count);
                Assert.AreEqual(newestPositionTimestamp.ToLongTimeString(), lvpList.First().PositionTimestamp.ToLongTimeString());                                
             }
        }

        [Test]
        public void PostProbeData_UpdateVehiclePosition_Success()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork imuow = new UnitOfWork(idtoFakeContext))
            {
                AddTripToRepo(imuow);

                LastVehiclePosition lvp = new LastVehiclePosition
                {
                    VehicleName = "MDT2",
                    PositionTimestamp = DateTime.UtcNow,
                    Latitude = 44.646581369493,
                    Longitude = -96.6830267664,
                    Speed = 10,
                    Heading = 180,
                    Accuracy = 5
                };

                imuow.Repository<LastVehiclePosition>().Insert(lvp);
                imuow.Save();

                DateTime newestPositionTimestamp = DateTime.UtcNow.AddMinutes(1);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long newestTimeStamp = Convert.ToInt64((newestPositionTimestamp - epoch).TotalMilliseconds);

                ProbeVehicleData probeData = new ProbeVehicleData { InboundVehicle = "MDT2" };
                PositionSnapshot ps = new PositionSnapshot()
                {
                    Accuracy = 5,
                    Altitude = 123,
                    Heading = 180,
                    Latitude = 44.646581369493,
                    Longitude = -96.6830267664,
                    Satellites = 0,
                    Speed = 14.77999305725097,
                    TimeStamp = newestTimeStamp
                };
                probeData.Positions.Add(ps);

                var mockTable = new Mock<IAzureTable<ProbeSnapshotEntry>>();
                var cut = new ProbeController(idtoFakeContext, mockTable.Object);

                SetupControllerForTests(cut);

                HttpResponseMessage returnMessage = cut.PostProbeData(probeData);

                List<LastVehiclePosition> lvpList = imuow.Repository<LastVehiclePosition>().Query().Get().Where(v => v.VehicleName == "MDT2").ToList();

                Assert.AreEqual(1, lvpList.Count);
                Assert.AreEqual(newestPositionTimestamp.ToLongTimeString(), lvpList.First().PositionTimestamp.ToLongTimeString());                
            }
        }

       // [Test]
       // public void PostProbeData_InvalidTripId_Fails()
       // {
       //using (IDbContext idtoFakeContext = new IDTOFakeContext())
       //      using (IUnitOfWork imuow = new UnitOfWork(idtoFakeContext))
       //      {
       //          AddTripToRepo(imuow);
       //     string tripId = "45";//bogus id
       //     ProbeVehicleData probeData = new ProbeVehicleData { TripID = tripId };
       //     PositionSnapshot ps = new PositionSnapshot()
       //     {
       //         Accuracy = 5,
       //         Altitude = 123,
       //         Heading = 180,
       //         Latitude = 44.646581369493,
       //         Longitude = -96.6830267664,
       //         Satellites = 0,
       //         Speed = 14.77999305725097,
       //         TimeStamp = 137349837631
       //     };
       //     probeData.Positions.Add(ps);

           

       //        var mockTable = new Mock<IAzureTable<ProbeSnapshotEntry>>();
       //        var cut = new ProbeController(idtoFakeContext, mockTable.Object);

       //        SetupControllerForTests(cut);

       //        HttpResponseMessage returnMessage = cut.PostProbeData(probeData);

       //        Assert.AreEqual(HttpStatusCode.NotFound, returnMessage.StatusCode);
       //    }
       // }
        [Test]
        public void PostProbeData_MissingInboundVehicle_Fails()
        {
            ProbeVehicleData probeData = new ProbeVehicleData();

            PositionSnapshot ps = new PositionSnapshot()
            {
                Accuracy = 5,
                Altitude = 123,
                Heading = 180,
                Latitude = 44.646581369493,
                Longitude = -96.6830267664,
                Satellites = 0,
                Speed = 14.77999305725097,
                TimeStamp = 137349837631
            };
            probeData.Positions.Add(ps);

            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork imuow = new UnitOfWork(idtoFakeContext))
            {

                var mockTable = new Mock<IAzureTable<ProbeSnapshotEntry>>();
                var cut = new ProbeController(idtoFakeContext, mockTable.Object);

                SetupControllerForTests(cut);

                HttpResponseMessage returnMessage = cut.PostProbeData(probeData);

                Assert.AreEqual(HttpStatusCode.BadRequest, returnMessage.StatusCode);
            }
        }
    }
}
