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
using IDTO.DataProcessor.TConnectMonitor;

using IDTO.DataProcessor;
using IDTO.BusScheduleInterface.Fakes;


namespace IDTO.UnitTests.IDTO.DataProcessor
{
      [TestFixture]
    public class TConnectMonitorWorkerTest : TConnectMonitorWorker
      {
          public TConnectMonitorWorkerTest()
              : base(new List<BusScheduleInterface.IBusSchedule>(new StubIBusSchedule[]{new StubIBusSchedule()}))
          {

          }

          [Test]
          public void testwithrealrepo_Passes()
          {
         
              //need to use Include, lazy loading is off
              using (IDbContext idtoFakeContext = new IDTOContext(@"Server=tcp:sfkee7y99k.database.windows.net,1433;Database=IDTO_Test;User ID=vital_admin@sfkee7y99k;Password=Super$0813^Monkey;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"))
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  int firstrandomid = unitOfWork.Repository<TConnect>().Query().Get().Select(i=>i.Id).First();
                  var t = unitOfWork.Repository<TConnect>().Query().Include(i => i.InboundStep.Trip.Traveler).Get()
                                 .Where(code => code.Id.Equals(firstrandomid)).First();
                  //vehiclenname line will fail if the below command is used instead.
                  //var t = unitOfWork.Repository<TConnect>().Query().Get()
                  //                .Where(code => code.Id.Equals(firstrandomid)).First();
                  string vehicleName = t.InboundStep.Trip.Traveler.LastName;

                  TConnectMonitorWorker worker = new TConnectMonitorWorker(null);
                  worker.SetupNewTConnects(unitOfWork); 
              }
          }
          [Test]
          public void MonitorTConnect_NoExistingTConnect_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt);
                  DateTime newEta = steps[0].EndDate;
                  MonitorTConnect(unitOfWork, mt, newEta);

                  var treq = unitOfWork.Repository<TConnectRequest>().Query().Get();
                  Assert.AreEqual(0, treq.Count(), "No TConnectRequest should be created.");
              }
          }
          [Test]
          public void MonitorTConnect_UpdateExistingTConnect_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt);
                  DateTime newEta = ((DateTime)mt.StartWindow).AddMinutes(3);

                  //Add a TconnectRequest so the path follows the update path
                  TConnectRequest newRequest = new TConnectRequest();


                  //newRequest.TConnectedVehicleId = GetOrCreateTConnectedVehicleKey(Uow, outboundStep);
                  newRequest.TConnectId = mt.Id;
                  newRequest.EstimatedTimeArrival = newEta;
                  //Calculate how late we'll be
                  newRequest.RequestedHoldMinutes = 2;
                  newRequest.TConnectStatusId = (int)TConnectStatuses.New;//new request                           
                  unitOfWork.Repository<TConnectRequest>().Insert(newRequest);
                  unitOfWork.Save();

                  MonitorTConnect(unitOfWork, mt, newEta);

                  var treq = unitOfWork.Repository<TConnectRequest>().Query().Get();
                  Assert.AreEqual(1, treq.Count(), "Only one TConnectRequest should be existing.");
                  TConnectRequest request = treq.First();
                  Assert.AreEqual(3, request.RequestedHoldMinutes, "Existing TConnectRequest should have been updated to 3 minutes");
                  Assert.AreEqual((int)TConnectStatuses.New, request.TConnectStatusId, "Status should be new");
              }
          }

          [Test]
          public void SendStartTripNotification_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  Trip trip;

                  SetupUowForNotificationTest(unitOfWork, out trip, out steps);
                  SendTripStartNotifications(unitOfWork);
              }
          }
 
          /*
          [Test]
          public void MonitorTConnect_UpdateExistingTConnect_2ndRequest_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt);
                  DateTime newEta = ((DateTime)mt.StartWindow).AddMinutes(3);

                  //Add a TconnectRequest so the path follows the update path
                  TConnectRequest newRequest = new TConnectRequest();
                  newRequest.TConnectedVehicleId = 2;
                  newRequest.TConnectId = mt.Id;
                  newRequest.EstimatedTimeArrival = newEta;
                  newRequest.RequestedHoldMinutes = 2;
                  newRequest.TConnectStatusId = (int)TConnectStatuses.New;                         
                  unitOfWork.Repository<TConnectRequest>().Insert(newRequest);
                  unitOfWork.Save();

                  //Add another tconnectrequest for another traveler for the same vehicle
                  TConnectRequest anotherReqForSameVehicleAccepted = new TConnectRequest();
                  anotherReqForSameVehicleAccepted.TConnectedVehicleId = 2;
                  anotherReqForSameVehicleAccepted.TConnectStatusId = (int)TConnectStatuses.Accepted;
                  anotherReqForSameVehicleAccepted.RequestedHoldMinutes = 7;//this one has been accepted for 7 minutes.
                  anotherReqForSameVehicleAccepted.TConnectId = 5;
                  unitOfWork.Repository<TConnectRequest>().Insert(anotherReqForSameVehicleAccepted);
                  TConnectRequest anotherReqForSameVehicleAccepted2 = new TConnectRequest();//test to make sure we are grabbing the greater time of the two.
                  anotherReqForSameVehicleAccepted2.TConnectedVehicleId = 2;
                  anotherReqForSameVehicleAccepted2.TConnectStatusId = (int)TConnectStatuses.Accepted;
                  anotherReqForSameVehicleAccepted2.RequestedHoldMinutes = 1;//this one has been accepted for 1 minutes.
                  anotherReqForSameVehicleAccepted2.TConnectId = 8;
                  unitOfWork.Repository<TConnectRequest>().Insert(anotherReqForSameVehicleAccepted2);
                  unitOfWork.Save();
                  MonitorTConnect(unitOfWork, mt, newEta);

                  var treq = unitOfWork.Repository<TConnectRequest>().Query().Get();
                  TConnectRequest request = treq.Where(t => t.TConnectId.Equals(mt.Id)).First();
                  Assert.AreEqual(3, request.RequestedHoldMinutes, "Existing TConnectRequest should have been upated to 3 minutes");
                  Assert.AreEqual((int)TConnectStatuses.Accepted, request.TConnectStatusId, "Status should be Accepted");
              }
          }
          */
                [Test]
          public void MonitorTConnect_UpdateExistingTConnect_AutoAccept_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt);
                  DateTime newEta = ((DateTime)mt.StartWindow).AddMinutes(5);

                  //Add a TconnectRequest so the path follows the update path
                  TConnectRequest newRequest = new TConnectRequest();
                  newRequest.TConnectedVehicleId = 2;
                  newRequest.TConnectId = mt.Id;
                  newRequest.EstimatedTimeArrival = newEta;
                  newRequest.RequestedHoldMinutes = 2;
                  newRequest.TConnectStatusId = (int)TConnectStatuses.New;                         
                  unitOfWork.Repository<TConnectRequest>().Insert(newRequest);
                  unitOfWork.Save();

                  //Add another tconnectrequest for another traveler for the same vehicle
                  TConnectedVehicle v = new TConnectedVehicle();
                  v.Id = 2;
                  v.CurrentAcceptedHoldMinutes = 7;
                  unitOfWork.Repository<TConnectedVehicle>().Insert(v);
                  unitOfWork.Save();

                  MonitorTConnect(unitOfWork, mt, newEta);

                  var treq = unitOfWork.Repository<TConnectRequest>().Query().Get();
                  TConnectRequest request = treq.Where(t => t.TConnectId.Equals(mt.Id)).First();
                  Assert.AreEqual(5, request.RequestedHoldMinutes, "Existing TConnectRequest should have been upated to 3 minutes");
                  Assert.AreEqual((int)TConnectStatuses.Accepted, request.TConnectStatusId, "Status should be Accepted");
              }
          }
          [Test]
          public void CreateNewTConnectRequestIfNeeded_AddsRequest_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt); 
                  
                  DateTime newEta = steps[0].EndDate.AddMinutes(11);//make sure we're late
                  CreateNewTConnectRequestIfNeeded(unitOfWork, mt, newEta, steps[2]);

                  TConnectRequest treq = unitOfWork.Repository<TConnectRequest>().Query().Get().First();
                  Assert.AreEqual(5, treq.RequestedHoldMinutes);
                  TConnectedVehicle tveh = unitOfWork.Repository<TConnectedVehicle>().Query().Get().First();
                  Assert.AreEqual(0,tveh.CurrentAcceptedHoldMinutes,"TConnectedVehicle instance should be created, but still have an accepted time of 0 since no requests have been accepted yet.");
              }
          }
          [Test]
          public void CreateNewTConnectRequestIfNeeded_2ndRqstForVehicle_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt);

                  //Add a TConnectedVehicle row to simulate someone else already creating it,so that
                  //the one we add will grab a reference to this one instead of making one.
                  TConnectedVehicle newTv = new TConnectedVehicle();
                  newTv.OriginallyScheduledDeparture = steps[2].StartDate;
                  newTv.TConnectStopCode = steps[2].FromStopCode;
                  newTv.ModifiedBy = "test";
                  newTv.ModifiedDate = DateTime.UtcNow;
                  unitOfWork.Repository<TConnectedVehicle>().Insert(newTv);
                  unitOfWork.Save();

                  DateTime newEta = steps[0].EndDate.AddMinutes(11);//make sure we're late
                  CreateNewTConnectRequestIfNeeded(unitOfWork, mt, newEta, steps[2]);

                  TConnectRequest treq = unitOfWork.Repository<TConnectRequest>().Query().Get().First();
                  Assert.AreEqual(5, treq.RequestedHoldMinutes);
                  TConnectedVehicle tveh = unitOfWork.Repository<TConnectedVehicle>().Query().Get().First();
                  Assert.AreEqual(0, tveh.CurrentAcceptedHoldMinutes, "TConnectedVehicle instance should be created, but still have an accepted time of 0 since no requests have been accepted yet.");
              }
          }
          [Test]
          public void CreateNewTConnectRequestIfNeeded_NotNeeded_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt);

                  DateTime newEta = steps[0].EndDate;
                  CreateNewTConnectRequestIfNeeded(unitOfWork, mt, newEta, steps[2]);

                  var treq = unitOfWork.Repository<TConnectRequest>().Query().Get();

                  Assert.AreEqual(0, treq.Count(),"No TConnectRequest should be created.");
              }
          }
          [Test]
          public void CreateNewTConnectRequestIfNeeded_TooLate_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  List<Step> steps;
                  TConnect mt;
                  Trip trip;
                  SetupUowWithTripAndTConnect(unitOfWork, out trip, out steps, out mt);

                  DateTime newEta = steps[0].EndDate.AddMinutes(25);//make sure we're really late
                  CreateNewTConnectRequestIfNeeded(unitOfWork, mt, newEta, steps[2]);

                  var treq = unitOfWork.Repository<TConnectRequest>().Query().Get();
                  Assert.AreEqual(0, treq.Count(), "No TConnectRequest should be created.");
                  var mtnew = unitOfWork.Repository<TConnect>().Query().Get().First();
                  Assert.AreEqual((int)TConnectStatuses.Done,mtnew.TConnectStatusId,"Status of a missed connection should be Done.");
              }
          }

          /// <summary>
          /// Note, this setup is a bit 'integration test-y' because I call SaveTrip to load up all the stuff and the Tconnect.
          /// </summary>
          /// <param name="unitOfWork"></param>
          /// <param name="tripEntity"></param>
          /// <param name="steps"></param>
          /// <param name="mt"></param>
          private void SetupUowWithTripAndTConnect(IUnitOfWork unitOfWork, out Trip tripEntity, out List<Step> steps, out TConnect mt)
          {
              Diagnostics = new IdtoDiagnostics();//to avoid null ref exc. set up base diagnostics.
               tripEntity = TestData.GetTrip();
              steps = TestData.GetSteps();//trips and steps get added in savetrip service call
              TConnectOpportunity TConnOpp = TestData.GetTConnectOpportunity();
              unitOfWork.Repository<TConnectOpportunity>().Insert(TConnOpp);
              unitOfWork.Save();

              ITripService tripService = new TripService(5);
              int id = tripService.SaveTrip(tripEntity, steps, unitOfWork);
              //Get the TConnect just created by the SaveTrip function
              mt = unitOfWork.Repository<TConnect>().Query().Get().First();
              Assert.AreEqual(1, unitOfWork.Repository<TConnect>().Query().Get().Count(), "TConnect that should have been created by SaveTrip did not get created.");

          }

          private void SetupUowForNotificationTest(IUnitOfWork unitOfWork, out Trip tripEntity, out List<Step> steps)
          {
              Diagnostics = new IdtoDiagnostics();//to avoid null ref exc. set up base diagnostics.
              tripEntity = TestData.GetTripForNotificationTest();
              steps = TestData.GetSteps();//trips and steps get added in savetrip service call

              ITripService tripService = new TripService(5);
              int id = tripService.SaveTrip(tripEntity, steps, unitOfWork);

          }
          [Test]
          public void SetupNewTConnects_Passes()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  Traveler traveler = new Traveler();
                  traveler.Id = 1;
                  traveler.LastName = "MDT2";

                  Trip trip = new Trip();
                  trip.Id = 1;
                  trip.TripStartDate = DateTime.Parse("1/1/2014 10:40");
                  trip.TravelerId = traveler.Id;
                 trip.Traveler = traveler;//must set object for navigation property or lazy loading wont work for mock.


                  Step stepEntity = new Step();
                  stepEntity.Id = 1;
                  stepEntity.StepNumber = 1;
                  stepEntity.TripId = trip.Id;
                  stepEntity.StartDate = DateTime.Parse("1/1/2014 10:02");
                  stepEntity.EndDate = DateTime.Parse("1/1/2014 10:40");
                  stepEntity.FromName = "Quartz Street";
                  stepEntity.FromProviderId = (int)Providers.CapTrans;
                  stepEntity.FromStopCode = "1001";
                  stepEntity.ModeId = (int)Modes.Bus;
                  stepEntity.RouteNumber = "039";
                  stepEntity.Distance = (decimal)12.2;
                  stepEntity.ToName = "Slate Run Road";
                  stepEntity.ToProviderId = (int)Providers.CapTrans;
                  stepEntity.ToStopCode = "2002";
                  stepEntity.Trip = trip;//must set object for navigation property or lazy loading wont work for mock.

                  TConnect tconn = new TConnect();
                  tconn.Id = 1;
                  tconn.TConnectStatusId = (int)TConnectStatuses.New;
                  tconn.InboundStepId = stepEntity.Id;
                  tconn.InboundStep = stepEntity;//must set object for navigation property or lazy loading wont work for mock.

                  unitOfWork.Repository<Traveler>().Insert(traveler);
                  
                  unitOfWork.Repository<Trip>().Insert(trip);
                 
                  unitOfWork.Repository<Step>().Insert(stepEntity);
             
                  unitOfWork.Repository<TConnect>().Insert(tconn);
                  unitOfWork.Save();

                  List<TConnect> newTconnects = unitOfWork.Repository<TConnect>().Query().Include(i => i.InboundStep.Trip.Traveler).Get()
                           .Where(s => s.TConnectStatusId == (int)TConnectStatuses.New).ToList();

                  TConnectMonitorWorker worker = new TConnectMonitorWorker(null);
                  worker.SetupNewTConnects(unitOfWork);

                  List<TConnect> monitoredTConnects = unitOfWork.Repository<TConnect>().Query().Get()
                      .Where(s => s.TConnectStatusId == (int)TConnectStatuses.Monitored).ToList();

                  Assert.AreEqual(1, monitoredTConnects.Count);
              }
          }
          /// <summary>
          /// Setup trip in future, should stay as 'New'
          /// </summary>
          [Test]
          public void SetupNewTConnects_NoAction()
          {
              using (IDbContext idtoFakeContext = new IDTOFakeContext())
              using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
              {
                  Traveler traveler = new Traveler();
                  traveler.Id = 1;
                  traveler.LastName = "MDT2";

                  Trip trip = new Trip();
                  trip.Id = 1;
                  trip.TripStartDate = DateTime.UtcNow.AddMinutes(20);//make in future
                  trip.TravelerId = traveler.Id;
                  trip.Traveler = traveler;//must set object for navigation property or lazy loading wont work for mock.


                  Step stepEntity = new Step();
                  stepEntity.Id = 1;
                  stepEntity.StepNumber = 1;
                  stepEntity.TripId = trip.Id;

                  stepEntity.Trip = trip;//must set object for navigation property or lazy loading wont work for mock.

                  TConnect tconn = new TConnect();
                  tconn.Id = 1;
                  tconn.TConnectStatusId = (int)TConnectStatuses.New;
                  tconn.InboundStepId = stepEntity.Id;
                  tconn.InboundStep = stepEntity;//must set object for navigation property or lazy loading wont work for mock.

                  unitOfWork.Repository<Traveler>().Insert(traveler);

                  unitOfWork.Repository<Trip>().Insert(trip);

                  unitOfWork.Repository<Step>().Insert(stepEntity);

                  unitOfWork.Repository<TConnect>().Insert(tconn);
                  unitOfWork.Save();

                  List<TConnect> newTconnects = unitOfWork.Repository<TConnect>().Query().Include(i => i.InboundStep.Trip.Traveler).Get()
                           .Where(s => s.TConnectStatusId == (int)TConnectStatuses.New).ToList();

                  TConnectMonitorWorker worker = new TConnectMonitorWorker(new List<BusScheduleInterface.IBusSchedule>(new StubIBusSchedule[]{new StubIBusSchedule()}));
                  worker.SetupNewTConnects(unitOfWork);

                  List<TConnect> monitoredTConnects = unitOfWork.Repository<TConnect>().Query().Get()
                      .Where(s => s.TConnectStatusId == (int)TConnectStatuses.New).ToList();

                  Assert.AreEqual(1, monitoredTConnects.Count);
              }
          }
   
    }
}
