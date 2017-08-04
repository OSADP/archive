using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Data;
using Microsoft.WindowsAzure.Storage;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using IDTO.Service;
using IDTO.Common;
using IDTO.Common.Storage;
using Moq;
using NUnit.Framework;
using IDTO.DataProcessor.TConnectMonitor;
using IDTO.DataProcessor;//workerrole
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.QualityTools.Testing.Fakes;
//using Microsoft.WindowsAzure.StorageClient;
//using Microsoft.WindowsAzure.Storage.Fakes;
using Microsoft.WindowsAzure;
//using Microsoft.WindowsAzure;
//using Microsoft.WindowsAzure.Fakes;
//using Microsoft.WindowsAzure.ServiceRuntime.Fakes;

namespace IDTO.UnitTests.IDTO.DataProcessor
{
    public class TConnectIntegrationTest
    {
        private readonly string realContextConnectionString = @"Server=tcp:sfkee7y99k.database.windows.net,1433;Database=IDTO_Test;User ID=IDTO_ServiceUser@sfkee7y99k;Password=12Bu$y0-4B3@ver13;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        private readonly string realCloudStorageAccount = "DefaultEndpointsProtocol=https;AccountName=idtodev;AccountKey=aB1n/kG3j07fJ9VvHJLejMWnQ2hLBhKXeEJAv1wJj/YXbKVFA6dP25ALfokDWjl0WQfdTpMzIHQR+z0aAITMow==";
        /// <summary>
        /// Cloud Storage Studio filter query to see MDT2:  (PartitionKey eq 'MDT2')
        /// </summary>
        private IAzureTable<ProbeSnapshotEntry> _probeTable;
        //[Test]
        //public void IntegrationTest_CapTrans()
        //{
        //    using (ShimsContext.Create())
        //    {
        //        WorkerRole workerRole = Setup();

        //        using (IDbContext context = new IDTOContext(realContextConnectionString))
        //        using (IUnitOfWork Uow = new UnitOfWork(context))
        //        {
        //            Traveler tr;
        //            Trip t;
        //            List<Step> steps;
        //            SetupBaseTConnectOpp_CapTransTrav_Trip_Steps(Uow, out tr, out t, out steps);

        //            SetupHalfHourBus15MinWalk15MinWaitHalfHourBus_Probe3MinAwayAtLatestAllowableTime(tr, steps);


        //            ITripService tripService = new TripService(5);
        //            int id = tripService.SaveTrip(t, steps, Uow);

        //            //Act
        //            workerRole.Run();
        //        }
        //    }
        //}
        [Test]
        public void IntegrationTest_CapTrans()
        {
            WorkerRole workerRole = new WorkerRole();
            int tripid = -1;
            Thread th = new Thread(new ThreadStart(() =>
            {
                using (ShimsContext.Create())
                {
                    workerRole = Setup();

                    using (IDbContext context = new IDTOContext(realContextConnectionString))
                    using (IUnitOfWork Uow = new UnitOfWork(context))
                    {
                        //First, delete all existing nunit data or we accumulate duplicates
                        ClearOutPreviousNUnitTests(Uow);

                        Traveler tr;
                        Trip t;
                        List<Step> steps;
                        SetupBaseTConnectOpp_CapTransTrav_Trip_Steps(Uow, out tr, out t, out steps);

                        SetupHalfHourBus5MinWalk25MinWaitHalfHourBus_Probe4MinAwayAtLatestAllowableTime(tr, steps);


                        ITripService tripService = new TripService(5);
                        tripid = tripService.SaveTrip(t, steps, Uow);

                        //Act
                        workerRole.Run();


                    }
                }
            }));

            th.Start();

            Thread.Sleep(60000);//give the test time to run (deletion of old test results takes a bit, then running new test )
            workerRole.OnStop();
            Thread.Sleep(10000);//give it time to stop.
            using (IDbContext context = new IDTOContext(realContextConnectionString))
            using (IUnitOfWork Uow = new UnitOfWork(context))
            {
                var createdrequest = Uow.Repository<TConnectRequest>().Query().Include(r => r.TConnect.InboundStep.Trip).Get().Where(k => k.TConnect.InboundStep.Trip.Id.Equals(tripid)).First();
                Assert.AreEqual(5, createdrequest.RequestedHoldMinutes);
            }

        }
        [Test]
        public void IntegrationTest_CapTrans_NoWalkStep()
        {
            WorkerRole workerRole = new WorkerRole();
            int tripid = -1;
            Thread th = new Thread(new ThreadStart(() =>
       {
           using (ShimsContext.Create())
           {
               workerRole = Setup();

               using (IDbContext context = new IDTOContext(realContextConnectionString))
               using (IUnitOfWork Uow = new UnitOfWork(context))
               {
                   //First, delete all existing nunit data or we accumulate duplicates
                   ClearOutPreviousNUnitTests(Uow);

                   Traveler tr;
                   Trip t;
                   List<Step> steps;
                   SetupBaseTConnectOpp_CapTransTrav_Trip_Steps(Uow, out tr, out t, out steps);

                   Setup17MinBus3MinWait20MinBus_Probe4MinAwayAt1MinToSpare(tr, steps);


                   ITripService tripService = new TripService(5);
                   tripid = tripService.SaveTrip(t, steps, Uow);

                   //Act
                   workerRole.Run();


               }
           }
       }));

            th.Start();

            Thread.Sleep(60000);//give the test time to run (deletion of old test results takes awhile, then running new test )
            workerRole.OnStop();
            Thread.Sleep(10000);//give it time to stop.
            using (IDbContext context = new IDTOContext(realContextConnectionString))
            using (IUnitOfWork Uow = new UnitOfWork(context))
            {
                var createdrequest = Uow.Repository<TConnectRequest>().Query().Include(r => r.TConnect.InboundStep.Trip).Get().Where(k => k.TConnect.InboundStep.Trip.Id.Equals(tripid)).First();
                Assert.AreEqual(3, createdrequest.RequestedHoldMinutes);
            }

        }
        /// <summary>
        /// Two TConnectRequests for the same vehicle should have a shared TConnectedVehicle entity.
        /// </summary>
        [Test]
        public void IntegrationTest_CapTrans_TwoTConnectsForSameVehicle()
        {
            WorkerRole workerRole = new WorkerRole();
            int tripid = -1;
            int tripid2 = -1;
            DateTime nowish  = DateTime.UtcNow;
            Thread th = new Thread(new ThreadStart(() =>
            {
                using (ShimsContext.Create())
                {
                    workerRole = Setup();

                    using (IDbContext context = new IDTOContext(realContextConnectionString))
                    using (IUnitOfWork Uow = new UnitOfWork(context))
                    {
                        //First, delete all existing nunit data or we accumulate duplicates
                        ClearOutPreviousNUnitTests(Uow);

                        Traveler tr;
                        Trip t;
                        List<Step> steps;
                        SetupBaseTConnectOpp_CapTransTrav_Trip_Steps(Uow, out tr, out t, out steps);

                        SetupHalfHourBus5MinWalk25MinWaitHalfHourBus_Probe4MinAwayAtLatestAllowableTime(tr, steps);


                        ITripService tripService = new TripService(5);
                        tripid = tripService.SaveTrip(t, steps, Uow);

                        Traveler tr2;
                        Trip t2;
                        List<Step> steps2;

                        tr2 = LoadDifferentTraveler(Uow);
                        GetTripAndSteps_CapTrans(tr2, out t2, out steps2);
              
                        Setup17MinBus3MinWait20MinBus_Probe4MinAwayAt1MinToSpare(tr2, steps2);


                              nowish= steps2[1].StartDate=steps[2].StartDate;//outbound steps must have identical start date
                
                        tripid2 = tripService.SaveTrip(t2, steps2, Uow);

                        //Act
                        workerRole.Run();


                    }
                }
            }));

            th.Start();

            Thread.Sleep(60000);//give the test time to run (deletion of old test results takes a bit, then running new test )
            workerRole.OnStop();
            Thread.Sleep(10000);//give it time to stop.
            using (IDbContext context = new IDTOContext(realContextConnectionString))
            using (IUnitOfWork Uow = new UnitOfWork(context))
            {
                var createdrequest = Uow.Repository<TConnectRequest>().Query().Include(r => r.TConnect.InboundStep.Trip).Get().Where(k => k.TConnect.InboundStep.Trip.Id.Equals(tripid)).First();
                Assert.AreEqual(5, createdrequest.RequestedHoldMinutes); 
                
                var createdrequest2 = Uow.Repository<TConnectRequest>().Query().Include(r => r.TConnect.InboundStep.Trip).Get().Where(k => k.TConnect.InboundStep.Trip.Id.Equals(tripid2)).First();
                Assert.AreEqual(3, createdrequest2.RequestedHoldMinutes);

                var v = Uow.Repository<TConnectedVehicle>().Query().Get().Where(m => m.OriginallyScheduledDeparture.Day.Equals(nowish.Day)
                    && m.OriginallyScheduledDeparture.Hour.Equals(nowish.Hour)
                     && m.OriginallyScheduledDeparture.Minute.Equals(nowish.Minute)
                      && m.OriginallyScheduledDeparture.Second.Equals(nowish.Second));
                Assert.AreEqual(1, v.Count());
            }

        }
    
        private WorkerRole Setup()
        {
            //Arrange
           // ShimRoleEnvironment.GetConfigurationSettingValueString = (key) =>
           // {
           //     if (key == "IDTOContext")
           //     {
           //         return realContextConnectionString;

           //     }
           //     if (key == "StorageConnectionString")
           //     {
           //         return realCloudStorageAccount;

           //     }
           //     if (key == "RunTConnectMonitor")
           //     {
           //         return "true";

           //     }
           //     if (key == "TConnectMonitorSleepTime")//sleep time in seconds
           //     {
           //         return "5";

           //     }
           //     if (key == "RunVehicleLocationMonitor")
           //     {
           //         return "false";

           //     }
           //     else
           //     {
           //         return "mockedSettingValue";
           //     }
           // };
           // //ShimCloudStorageAccount.FromConfigurationSettingString = (key2) =>
           // //{                
           // //    return CloudStorageAccount.Parse(realCloudStorageAccount);
           // //};
           // ShimCloudConfigurationManager.GetSettingString = (key2) =>
           // {                
           //     return  realCloudStorageAccount;
           //};
           // ShimCloudStorageAccount cloud = new ShimCloudStorageAccount(CloudStorageAccount.Parse(realCloudStorageAccount));
            WorkerRole workerRole = new WorkerRole();
            workerRole.SetUpBindingForDiagnostics();

            SetupProbeTable();
            return workerRole;
        }

        private static void SetupBaseTConnectOpp_CapTransTrav_Trip_Steps(IUnitOfWork Uow, out Traveler tr, out Trip t, out List<Step> steps)
        {


            tr = LoadTraveler(Uow);
            GetTripAndSteps_CapTrans(tr, out t, out steps);


            TConnectOpportunity TConnOpp = TestData.GetTConnectOpportunity();
            //override some values from default settings
            TConnOpp.CheckpointRoute = null;
            TConnOpp.TConnectRoute = null;
            TConnOpp.CheckpointStopCode = steps[0].ToStopCode;
            TConnOpp.TConnectStopCode = steps[2].FromStopCode;
            var alreadyexists = Uow.Repository<TConnectOpportunity>().Query().Get()
                .Where(s => s.CheckpointStopCode.Equals(TConnOpp.CheckpointStopCode) &&
                s.CheckpointRoute.Equals(TConnOpp.CheckpointRoute) && s.TConnectStopCode.Equals(TConnOpp.TConnectStopCode)
                && s.TConnectRoute.Equals(TConnOpp.TConnectRoute));
            if (alreadyexists.Count() == 0)
            {
                Uow.Repository<TConnectOpportunity>().Insert(TConnOpp);
                Uow.Save();
            }


        }

        private static void GetTripAndSteps_CapTrans(Traveler tr, out Trip t, out List<Step> steps)
        {
            t = TestData.GetTrip();
            t.TravelerId = tr.Id;
            steps = TestData.GetSteps();
            //Override some values from default settings.
            //CapTrans doesn't have stop codes.
           // steps[0].FromStopCode = steps[1].FromStopCode = null;
            steps[0].ToStopCode = "DCSCBRD1";
            steps[0].RouteNumber = steps[1].RouteNumber = steps[2].RouteNumber = null;
        }
        private static Traveler LoadDifferentTraveler(IUnitOfWork Uow)
        {
            Traveler tr;
            tr = TestData.GetTraveler_CapTrans();
            Uow.Repository<Traveler>().Insert(tr);
            Uow.Save();
            return tr;
        }
        private static Traveler LoadTraveler(IUnitOfWork Uow)
        {
            Traveler tr2;
            tr2 = TestData.GetTraveler_CapTrans();
            //Change some stuff to make this one distintive
            tr2.LastName = "MDT1";
            tr2.Email = "capt@mdt.com";
            Uow.Repository<Traveler>().Insert(tr2);
            Uow.Save();
            return tr2;
        }
        /// <summary>
        /// NUnit tests create travelers modified by 'NUnit'. Finding those, we can find all their derivative info
        /// and clean it out of the database.
        /// </summary>
        /// <param name="Uow"></param>
        private static void ClearOutPreviousNUnitTests(IUnitOfWork Uow)
        {
            var travelers = Uow.Repository<Traveler>().Query().Get().Where(t => t.ModifiedBy.Equals(TestData.ModifiedByString)).ToList();
            foreach (Traveler traveler in travelers)
            {
                var trips = Uow.Repository<Trip>().Query().Get().Where(t => t.TravelerId.Equals(traveler.Id)).ToList();
                foreach (Trip trip in trips)
                {
                    //find tconnects that reference the steps owned by this trip
                    var stepsForTrip = Uow.Repository<Step>().Query().Get().Where(s => s.TripId.Equals(trip.Id)).Select(i => i.Id).ToList();

                    List<TConnect> tconn = Uow.Repository<TConnect>().Query().Get().Where(t => stepsForTrip.Any(s => s.Equals(t.InboundStepId))).ToList();
                    int tConnectedVehicleId = -1;
                    foreach (TConnect tc in tconn)
                    {
                        //Need to delete TConnectedVehicle IF this is the only TConnectRequest that references it.
                        var req = Uow.Repository<TConnectRequest>().Query().Include(v => v.TConnectedVehicle).Get().Where(t => t.TConnectId.Equals(tc.Id)).ToList();
                        if (req.Count() > 0)
                        {
                            tConnectedVehicleId = req.First().TConnectedVehicleId;
                        }
                        //Delete the tconnect and tConnectREquest. Needs to happen BEFORE tconnected vehicle
                        if (req.Count() > 0)
                        {
                            Uow.Repository<TConnectRequest>().Delete(req.First());
                            Uow.Save();
                        }
                        Uow.Repository<TConnect>().Delete(tc);
                        Uow.Save();

                        if (tConnectedVehicleId != -1)
                        {

                            //ok, we have the vehicle id for this request. see if any other requests link to that same vehicle id.
                            var alsoRefVeh = Uow.Repository<TConnectRequest>().Query().Get().Where(t => t.TConnectedVehicleId.Equals(tConnectedVehicleId));
                            if (alsoRefVeh.Count() == 0)
                            {
                                //there is only one 
                                Uow.Repository<TConnectedVehicle>().Delete(tConnectedVehicleId);
                            }

                        }
                    }
                    foreach(int s in stepsForTrip){
                        Uow.Repository<Step>().Delete(s);

                    }
                    //Delete the trip (cascade delete will do the steps).
                    Uow.Repository<Trip>().Delete(trip);
                }
                //Delete the traveler last//cascade delete should do the trip and steps
                Uow.Repository<Traveler>().Delete(traveler);
                Uow.Save();
            }


            //   Trip trip = Uow.Repository<Trip>().Find(id);
            //   if (trip == null)
            //   {
            //       return NotFound();
            //   }
            //te the trip (cascade delete will do the steps).
            //   Uow.Repository<Trip>().Delete(trip);
            // Uow.Save();
        }
        /// <summary>
        /// :-)
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="steps"></param>
        private void SetupHalfHourBus5MinWalk25MinWaitHalfHourBus_Probe4MinAwayAtLatestAllowableTime(Traveler tr, List<Step> steps)
        {



            //Override times to make trip present tense
            //inbound step
            steps[0].StartDate = DateTime.UtcNow.AddMinutes(-60);
            steps[0].EndDate = DateTime.UtcNow.AddMinutes(-30);
            //walk step
            steps[1].StartDate = DateTime.UtcNow.AddMinutes(-30);
            steps[1].EndDate = DateTime.UtcNow.AddMinutes(-25);
            //tconnect step
            steps[2].StartDate = DateTime.UtcNow;
            steps[2].EndDate = DateTime.UtcNow.AddMinutes(30);

            DateTime probetimestamp = DateTime.UtcNow.AddMinutes(-5).AddSeconds(30);
            //Set up a probe snapshot coming from the James Rd Gate
            //It takes 4:01 minutes to travel between gates.
            ProbeSnapshotEntry newProbeSnapshot = new ProbeSnapshotEntry
            {
                PartitionKey = tr.LastName,//inbound vehicle name is last name of traveler
                RowKey = Guid.NewGuid().ToString(),

                PositionTimestamp = probetimestamp,
                Accuracy = 5,
                Altitude = 123,
                Heading = 180,
                Latitude = 39.977592,
                Longitude = -82.913054,
                Satellites = 0,
                Speed = 14.77999305725097
            };

            _probeTable.AddEntity(newProbeSnapshot);
        }
        private void Setup17MinBus3MinWait20MinBus_Probe4MinAwayAt1MinToSpare(Traveler tr, List<Step> steps)
        {
            //Override times to make trip present tense
            //inbound step
            steps[0].StartDate = DateTime.UtcNow.AddMinutes(-20);
            steps[0].EndDate = DateTime.UtcNow.AddMinutes(-3);
            //walk step
            steps.RemoveAt(1);
            //tconnect step
            steps[1].StartDate = DateTime.UtcNow;
            steps[1].EndDate = DateTime.UtcNow.AddMinutes(30);
            steps[1].StepNumber = 2;

            DateTime probetimestamp = DateTime.UtcNow.AddMinutes(-2).AddSeconds(30);
            //Set up a probe snapshot coming from the James Rd Gate
            //It takes 4:01 minutes to travel between gates.
            ProbeSnapshotEntry newProbeSnapshot = new ProbeSnapshotEntry
            {
                PartitionKey = tr.LastName,//inbound vehicle name is last name of traveler
                RowKey = Guid.NewGuid().ToString(),

                PositionTimestamp = probetimestamp,
                Accuracy = 5,
                Altitude = 123,
                Heading = 180,
                Latitude = 39.977592,
                Longitude = -82.913054,
                Satellites = 0,
                Speed = 14.77999305725097
            };

            _probeTable.AddEntity(newProbeSnapshot);
        }
        private void SetupProbeTable()
        {
            //setup probe table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(realCloudStorageAccount);

            _probeTable = new AzureTable<ProbeSnapshotEntry>(storageAccount);
            _probeTable.CreateIfNotExist();
        }
        /*
               [Test]
          public void IntegrationTest()
          {
              // ARRANGE

              //Connection string with valid shape, but that doesn't point to a real target
              string testConnectionString = "DefaultEndpointsProtocol=https;AccountName=fakeAccount;AccountKey=...";
              int deleteCounter = 0;
              int propAccessCounter = 0;
             // List<Book> deletedItems = new List<Book>();
            //List<Book> mockBookList = GetTestBookList();

              ManualResetEvent mre = new ManualResetEvent(false);

              Thread th = new Thread(new ThreadStart(() =>
              {
                  //Need to perform mock setup on thread running test....

                  //Mock this: RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString") 
                  //to instead always return
                  //connection string to a test storage account
                  Mock.SetupStatic<RoleEnvironment>();
                  Mock.Arrange<string>(() =>
                      RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")).
                      Returns(testConnectionString);

                  var r = new Mock<RoleEnvironment>();
                  r.Setup(m => m.(RoleEnvironment.GetConfigurationSettingValue)).Returns(testConnectionString);

                  //Mock this: CloudStorageAccount.Parse("...any string...") to 
                  //instead always return the same test storage account instance
                  CloudStorageAccount fixedTestAccount = CloudStorageAccount.Parse(testConnectionString);
                  Mock.SetupStatic<CloudStorageAccount>();
                  Mock.Arrange<CloudStorageAccount>(() => CloudStorageAccount.Parse(Arg.AnyString)).Returns(
                      fixedTestAccount
                      );

                  //Mock any calls to any instance of BooksTableServiceContext 
                  //that accesses Books property to return our test list instead.                    
                  CloudStorageAccount acct = CloudStorageAccount.Parse(
                      RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
                  BooksTableServiceContext booksCtx =
                      new BooksTableServiceContext(acct.TableEndpoint.ToString(), acct.Credentials);

                  //Simulate effect of actually removing items
                  Mock.Arrange(() => booksCtx.Books).Returns(() =>
                  {
                      //make test completion deterministic
                      if (propAccessCounter > 0)
                          mre.Set();

                      propAccessCounter++;

                      return mockBookList.Except(deletedItems).AsQueryable();
                  });

                  //Setup counter that counts how many times delete is called and tracks deleted items                    
                  Mock.Arrange(() => booksCtx.DeleteBook(Arg.IsAny<Book>())).DoInstead((Book b) =>
                  {
                      deleteCounter++;
                      deletedItems.Add(b);
                  });

                  // ACT
                  WorkerRole wr = new WorkerRole();
                  wr.Run();
              }));

              th.Start();

              mre.WaitOne();

              // ASSERT
              // Assert delete was called twice, and that deleted items were the expected ones
              Assert.AreEqual(2, deleteCounter);
              Assert.AreEqual(2, deletedItems.Count());
              CollectionAssert.AreEquivalent(
                  new List<Book> { mockBookList.ElementAt(1), mockBookList.ElementAt(2) },
                  deletedItems);
          }
      * */

    }
}
