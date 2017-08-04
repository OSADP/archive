using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using IDTO.Service;
using IDTO.Common;
using Moq;
using NUnit.Framework;
namespace IDTO.UnitTests.IDTO.WebAPI
{
    [TestFixture]
    public class TripServiceTest
    {
        private static void SetupControllerForTests()
        {
        }
       

        [Test]
        public void SaveTrip_1TConnect_Passes()
        {
             using (IDbContext idtoFakeContext = new IDTOFakeContext())
             using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
             {
                 Trip tripEntity = TestData.GetTrip();
              List<Step> steps = TestData.GetSteps();//trips and steps get added in savetrip service call

              TConnectOpportunity TConnOpp = TestData.GetTConnectOpportunity();
              unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp);

                //TConnectOpportunity TConnOpp2 = new TConnectOpportunity();
                //TConnOpp2.Id = 1;
                //TConnOpp2.ModifiedBy = "Nunit";
                //TConnOpp2.ModifiedDate = DateTime.UtcNow;
                //TConnOpp2.CheckpointProviderId = 1;
                //TConnOpp2.CheckpointStopCode = "2002";
                //TConnOpp2.CheckpointRoute = "";
                //TConnOpp2.TConnectProviderId = 1;
                //TConnOpp2.TConnectStopCode = "8888";
                //TConnOpp2.TConnectRoute = "426";
                //unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp2);

                unitOfWork.Save();
                 
                 ITripService tripService = new TripService(5);
                 int id = tripService.SaveTrip(tripEntity, steps, unitOfWork);

                 Assert.AreEqual(1, unitOfWork.Repository<TConnect>().Query().Get().Count());
                 Assert.AreEqual("Trip Created", unitOfWork.Repository<TripEvent>().Query().Get().First().Message);
                 Assert.AreEqual("T-Connect Created", unitOfWork.Repository<TripEvent>().Query().Get().Last().Message);
             }
        }



        [Test]
        public void SaveTrip_0TConnect_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Trip tripEntity = TestData.GetTrip();

                List<Step> steps = TestData.GetSteps();

                TConnectOpportunity TConnOpp = new TConnectOpportunity();
                TConnOpp.Id = 1;
                TConnOpp.ModifiedBy = "Nunit";
                TConnOpp.ModifiedDate = DateTime.UtcNow;
                TConnOpp.CheckpointProviderId = 1;
                TConnOpp.CheckpointStopCode = "5555";
                TConnOpp.CheckpointRoute = "";
                TConnOpp.TConnectProviderId = 1;
                TConnOpp.TConnectStopCode = "6666";
                TConnOpp.TConnectRoute = "426";
                unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp);

                TConnectOpportunity TConnOpp2 = new TConnectOpportunity();
                TConnOpp2.Id = 2;
                TConnOpp2.ModifiedBy = "Nunit";
                TConnOpp2.ModifiedDate = DateTime.UtcNow;
                TConnOpp2.CheckpointProviderId = 1;
                TConnOpp2.CheckpointStopCode = "7777";
                TConnOpp2.CheckpointRoute = "";
                TConnOpp2.TConnectProviderId = 1;
                TConnOpp2.TConnectStopCode = "8888";
                TConnOpp2.TConnectRoute = "426";
                unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp2);

                unitOfWork.Save();

                ITripService tripService = new TripService(5);
                int id = tripService.SaveTrip(tripEntity, steps, unitOfWork);

                Assert.AreEqual(0, unitOfWork.Repository<TConnect>().Query().Get().Count());
            }
        }

        [Test]
        public void SaveTrip_2TConnect_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Trip tripEntity = TestData.GetTrip();
                tripEntity.TripEndDate = DateTime.Parse("1/1/2014 11:32");
                //add fourth leg to trip to test steps loop, get back on the same stop, different bus route.
                List<Step> steps = TestData.GetSteps();
                Step stepEntity4 = new Step();
                stepEntity4.StepNumber = steps.Count + 1;
                stepEntity4.StartDate = DateTime.Parse("1/1/2014 11:02");
                stepEntity4.EndDate = DateTime.Parse("1/1/2014 11:32");
                stepEntity4.FromName = "Quarry Corner";
                stepEntity4.FromProviderId = (int)Providers.COTA;
                stepEntity4.FromStopCode = "4004";
                stepEntity4.ModeId = (int)Modes.Bus;
                stepEntity4.RouteNumber = "500";
                stepEntity4.Distance = (decimal)12.2;
                stepEntity4.ToName = "Calcite Creek Drive";
                stepEntity4.ToProviderId = (int)Providers.COTA;
                stepEntity4.ToStopCode = "5005";
                steps.Add(stepEntity4);

                TConnectOpportunity TConnOpp = new TConnectOpportunity();
                TConnOpp.Id = 1;
                TConnOpp.ModifiedBy = "Nunit";
                TConnOpp.ModifiedDate = DateTime.UtcNow;
                TConnOpp.CheckpointProviderId = 1;
                TConnOpp.CheckpointStopCode = "2002";
                TConnOpp.CheckpointRoute = "";
                TConnOpp.TConnectProviderId = 1;
                TConnOpp.TConnectStopCode = "3003";
                TConnOpp.TConnectRoute = "426";
                unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp);

                TConnectOpportunity TConnOpp2 = new TConnectOpportunity();
                TConnOpp2.Id = 1;
                TConnOpp2.ModifiedBy = "Nunit";
                TConnOpp2.ModifiedDate = DateTime.UtcNow;
                TConnOpp2.CheckpointProviderId = 1;
                TConnOpp2.CheckpointStopCode = "4004";
                TConnOpp2.CheckpointRoute = "";
                TConnOpp2.TConnectProviderId = 1;
                TConnOpp2.TConnectStopCode = "4004";
                TConnOpp2.TConnectRoute = "500";
                unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp2);

                unitOfWork.Save();

                ITripService tripService = new TripService(5);
                int id = tripService.SaveTrip(tripEntity, steps, unitOfWork);

                Assert.AreEqual(2, unitOfWork.Repository<TConnect>().Query().Get().Count());
            }
        }


        [Test]
        public void SaveTrip_TestEmptyRoute_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Trip tripEntity = TestData.GetTrip();
                List<Step> steps = TestData.GetSteps();//trips and steps get added in savetrip service call

                TConnectOpportunity TConnOpp = TestData.GetTConnectOpportunity();
                   steps[2].RouteNumber = ""; //clear out the route
                   TConnOpp.TConnectRoute = "";   //clear out the route
                
                unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp);



                unitOfWork.Save();

                ITripService tripService = new TripService(5);
                int id = tripService.SaveTrip(tripEntity, steps, unitOfWork);

                Assert.AreEqual(1, unitOfWork.Repository<TConnect>().Query().Get().Count());
            }
        }

    }
}
