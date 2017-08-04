using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Data;
using Microsoft.WindowsAzure.Storage.Table;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using IDTO.Service;
using IDTO.Common;
using Moq;
using NUnit.Framework;
using IDTO.DataProcessor.VehicleLocationMonitor;
using IDTO.Common.Storage;
using Microsoft.WindowsAzure;
using Ninject;

using System.Device.Location;
namespace IDTO.UnitTests.IDTO.DataProcessor
{
    [TestFixture]
    public class CapTransVehicleLocationTest : CapTransVehicleLocation
    {
        ///I derived the test class from the class-under-test because I didn't like
        ///the idea of making all those functions 'public' just to allow testing. Making
        /////them protected sat a little better with me, so I did that.
        

        public static IKernel Kernel = new StandardKernel();
        
        //Commented out as no other code was calling the method under test
        //[Test]
        //public void GetLatestLocationFromTable_Passes()
        //{
        //    string inboundvehicle = "MDT2";

        //    using (IDbContext idtoFakeContext = new IDTOFakeContext())
        //    using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
        //    {
        //        DateTime dtTemp = new DateTime(1970, 1, 1, 0, 0, 0);
        //        List<ProbeSnapshotEntry> entries = new List<ProbeSnapshotEntry>();
        //        IAzureTable<ProbeSnapshotEntry> mockTable = new Mock<IAzureTable<ProbeSnapshotEntry>>().Object;

        //        ProbeSnapshotEntry newProbeSnapshot = new ProbeSnapshotEntry
        //        {
        //            PartitionKey = inboundvehicle,
        //            RowKey = Guid.NewGuid().ToString(),
        //            Accuracy = 5,
        //            Altitude = 123,
        //            Heading = 180,
        //            Latitude = 44.646581369493,
        //            Longitude = -96.6830267664,
        //            Satellites = 0,
        //            Speed = 14.77999305725097,
        //            PositionTimestamp = dtTemp.AddMilliseconds(137349837631)
        //        };
        //        entries.Add(newProbeSnapshot);
        //        mockTable.AddEntity(newProbeSnapshot);

        //        var mock = new Mock<IAzureTable<ProbeSnapshotEntry>>();
        //        mock.SetupGet(t => t.Query).Returns(mockTable.Query);

        //        ProbeSnapshotEntry retProbeSnapshot = GetLatestLocationFromTable(inboundvehicle, mock.Object);
        //        Assert.AreEqual(retProbeSnapshot.Latitude, newProbeSnapshot.Latitude);
        //    }
        //}

        //Commented out as no other code was calling the method under test
        //[Test]
        //public void GetLatestLocationFromTable_ThrowsEx()
        //{
        //    string inboundvehicle = "MDT2";
        //    try
        //    {
        //        using (IDbContext idtoFakeContext = new IDTOFakeContext())
        //        using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
        //        {
        //            DateTime dtTemp = new DateTime(1970, 1, 1, 0, 0, 0);
        //            List<ProbeSnapshotEntry> entries = new List<ProbeSnapshotEntry>();
        //            IAzureTable<ProbeSnapshotEntry> mockTable = new Mock<IAzureTable<ProbeSnapshotEntry>>().Object;

        //            var mock = new Mock<IAzureTable<ProbeSnapshotEntry>>();

        //            ProbeSnapshotEntry retProbeSnapshot = GetLatestLocationFromTable(inboundvehicle, mock.Object);
        //            Assert.AreEqual(true, false, "Test should have thrown an exception and not executed this line.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.True(ex.Message.StartsWith("No ProbeSnapshotEntries found "));
                
        //    }
        //}
        [Test]
        public void CalcPointBEtaFromPointA_Passes()
        {
                using (IDbContext idtoFakeContext = new IDTOFakeContext())
                using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
                {
                    DateTime date = new DateTime(2014, 1, 1, 12, 0, 0);
                    GeoCoordinate currentLoc = new GeoCoordinate { Latitude = 39.975148, Longitude = -82.894379 };
                    GeoCoordinate destinationLoc = new GeoCoordinate { Latitude = 39.977592, Longitude = -82.913055 };
                    DateTime eta= CalcPointBEtaFromPointA(destinationLoc, date, currentLoc);
                    //Distance:	1.614 km =1614 m
                    float actualdist = 1615.81F;
                    //20 mph = 8.9mps
                    //15 Miles per Hour = 6.7056 Meters per Second
                    double seconds = 239; //actualdist / 6.7;
                    //Dont match down in the noise, but are good through seconds.
                    Assert.AreEqual(date.AddSeconds(seconds).Hour, eta.Hour);
                    Assert.AreEqual(date.AddSeconds(seconds).Minute, eta.Minute);
                    Assert.AreEqual(date.AddSeconds(seconds).Second, eta.Second);
                }
           
        }
        [Test]
        public void CalcPointAEtaFromPointA_Passes()
        {
      
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                DateTime date = new DateTime(2014, 1, 1, 12, 0, 0);
                GeoCoordinate currentLoc = new GeoCoordinate { Latitude = 39.975148, Longitude = -82.894379 };
                GeoCoordinate destinationLoc = new GeoCoordinate { Latitude = 39.875148, Longitude = -82.894379 };
                DateTime eta = CalcPointBEtaFromPointA(destinationLoc, date, currentLoc);
                //Distance:	1.614 km =1614 m
                float actualdist = 0;
                //20 mph = 8.9mps
                //double seconds = actualdist / 8.9;
                //Dont match down in the noise, but are good through seconds.
                Assert.AreEqual(12, eta.Hour);
                Assert.AreEqual(33, eta.Minute);
                Assert.AreEqual(0, eta.Second);
            }

        }
        [Test]
        public void GetCoordForOutboundStep_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Step inbound = new Step();
                inbound.Id = inbound.StepNumber = 1;
                inbound.StartDate = DateTime.Parse("1/1/2014 10:02");
                inbound.EndDate = DateTime.Parse("1/1/2014 10:40");
                inbound.FromName = "Quartz Street";
                inbound.FromProviderId = (int)Providers.CapTrans;
                inbound.FromStopCode = "1001";
                inbound.ModeId = (int)Modes.Bus;
                inbound.RouteNumber = "039";
                inbound.Distance = (decimal)12.2;
                inbound.ToName = "Broad St Gate";
                inbound.ToProviderId = (int)Providers.CapTrans;
                inbound.ToStopCode = "DCSCBRD1";

                Step outbound = new Step();
                outbound.Id = outbound.StepNumber = 2;
                outbound.StartDate = DateTime.Parse("1/1/2014 10:02");
                outbound.EndDate = DateTime.Parse("1/1/2014 10:40");
                outbound.FromName = "Broad St Gate";
                outbound.FromProviderId = (int)Providers.CapTrans;
                outbound.FromStopCode = "DCSCBRD1";
                outbound.ModeId = (int)Modes.Bus;
                outbound.RouteNumber = "039";
                outbound.Distance = (decimal)12.2;
                outbound.ToName = "Slate Run Road";
                outbound.ToProviderId = (int)Providers.CapTrans;
                outbound.ToStopCode = "DCSCBRD1";
                unitOfWork.Repository<Step>().Insert(inbound);
                unitOfWork.Save();

                TConnect tconn = new TConnect();
                tconn.InboundStepId = inbound.Id;
                tconn.OutboundStepId = outbound.Id;
                tconn.InboundStep = inbound;
                tconn.OutboundStep = outbound;

                unitOfWork.Repository<TConnect>().Insert(tconn);
                unitOfWork.Save();

                GeoCoordinate mainGate = new GeoCoordinate { Latitude = 39.975148, Longitude = -82.894379 };
                GeoCoordinate coord = GetCoordForInboundStep(tconn, unitOfWork);

                Assert.AreEqual(coord, mainGate);

            }
        }
             [Test]
        public void GetCoordForOutboundStep_ThrowsEx()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Step inbound = new Step();
                inbound.Id = inbound.StepNumber = 1;
                inbound.StartDate = DateTime.Parse("1/1/2014 10:02");
                inbound.EndDate = DateTime.Parse("1/1/2014 10:40");
                inbound.FromName = "Quartz Street";
                inbound.FromProviderId = (int)Providers.CapTrans;
                inbound.FromStopCode = "1001";
                inbound.ModeId = (int)Modes.Bus;
                inbound.RouteNumber = "039";
                inbound.Distance = (decimal)12.2;
                inbound.ToName = "Slate Run Road";
                inbound.ToProviderId = (int)Providers.CapTrans;
                inbound.ToStopCode = "2002";

                Step outbound = new Step();
                outbound.Id = outbound.StepNumber = 2;
                outbound.StartDate = DateTime.Parse("1/1/2014 10:02");
                outbound.EndDate = DateTime.Parse("1/1/2014 10:40");
                outbound.FromName = "NotMain Gate";//change gate FromName so that it does not match the list.
                outbound.FromProviderId = (int)Providers.CapTrans;
                outbound.FromStopCode = "1001";
                outbound.ModeId = (int)Modes.Bus;
                outbound.RouteNumber = "039";
                outbound.Distance = (decimal)12.2;
                outbound.ToName = "Slate Run Road";
                outbound.ToProviderId = (int)Providers.CapTrans;
                outbound.ToStopCode = "2002";
                unitOfWork.Repository<Step>().Insert(inbound);
                unitOfWork.Save();

                TConnect tconn = new TConnect();
                tconn.InboundStepId = inbound.Id;
                tconn.OutboundStepId = outbound.Id;
                tconn.InboundStep = inbound;
                tconn.OutboundStep = outbound;

                unitOfWork.Repository<TConnect>().Insert(tconn);
                unitOfWork.Save();

               Assert.Throws<NotImplementedException>(delegate { GetCoordForInboundStep(tconn, unitOfWork); });

            }
        }



    }
}
