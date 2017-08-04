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
using IDTO.DispatcherPortal.Controllers;
using IDTO.DispatcherPortal.Models;
using System.Data.Entity;

using IDTO.DispatcherPortal.Common;
using IDTO.DispatcherPortal.Common.Models;

namespace IDTO.UnitTests.IDTO.DispatcherPortal
{
    [TestFixture]
    public class TConnectedVehicleControllerTest
    {
        [Test]
        public async void CreateUser()
        {
            LoginManager loginManager = new LoginManager("https://idto-dev.azure-mobile.net/", "xyFSfirhoENlQSxJeQAfOnKzWVCEIn18");
            LoginResult loginResult = await loginManager.Register("lynx_dispatcher1@gmail.com", "G0LyNx!", "", "");

            Assert.AreEqual(true, loginResult.Success);
        }

        [Test]
        public void ListVehicles_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                //Setup
                TConnectedVehicle vehicleEnt1 = new TConnectedVehicle();
                vehicleEnt1.Id = 1;
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.Monitored };
                unitOfWork.Repository<TConnect>().Insert(tc);
                TConnectRequest reqEnt1 = new TConnectRequest();
                reqEnt1.TConnectedVehicleId = vehicleEnt1.Id;
                reqEnt1.TConnectId = tc.Id;
                reqEnt1.TConnectStatusId = (int)TConnectStatuses.New;

                unitOfWork.Repository<TConnectedVehicle>().Insert(vehicleEnt1);
                unitOfWork.Repository<TConnectRequest>().Insert(reqEnt1);
                unitOfWork.Save();

                TConnectedVehicle vehicleEnt2 = new TConnectedVehicle();
                vehicleEnt2.Id = 2;
                TConnect tc2 = new TConnect { Id = 2, TConnectStatusId = (int)TConnectStatuses.Monitored };
                unitOfWork.Repository<TConnect>().Insert(tc2);
                TConnectRequest reqEnt2 = new TConnectRequest();
                reqEnt2.TConnectedVehicleId = vehicleEnt2.Id;
                reqEnt2.TConnectId = tc2.Id;
                reqEnt2.TConnectStatusId = (int)TConnectStatuses.New;

                unitOfWork.Repository<TConnectedVehicle>().Insert(vehicleEnt2);
                unitOfWork.Repository<TConnectRequest>().Insert(reqEnt2);
                unitOfWork.Save();
               
                var bc = unitOfWork.Repository<TConnect>().Query().Get()
                     .Where(r => r.TConnectStatusId.Equals((int)TConnectStatuses.Monitored)).ToList();

                var vehicleidsWithNew = unitOfWork.Repository<TConnectRequest>().Query().Get().Include(r => r.TConnect)
    .Where(r => r.TConnectStatusId.Equals((int)TConnectStatuses.New)
   // && r.TConnect.TConnectStatusId.Equals((int)TConnectStatuses.Monitored)
    )
    .Select(r => r.TConnectedVehicleId).Distinct();
                int a = vehicleidsWithNew.Count();
  //.Include does not work on Fake Repository!
                TConnectedVehicleController tvCont = new TConnectedVehicleController(unitOfWork);
                List<TConnVehicleViewModel> vehicles = tvCont.GetVehiclesWithPendingRequests();
                Assert.AreEqual(2,vehicles.Count());
            }
        }
        [Test]
        public void ListVehicles_SameBus_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                //Setup
                TConnectedVehicle vehicleEnt1 = new TConnectedVehicle();
                vehicleEnt1.Id = 1;
                TConnectRequest reqEnt1 = new TConnectRequest();
                reqEnt1.TConnectedVehicleId = vehicleEnt1.Id;
                reqEnt1.TConnectStatusId = (int)TConnectStatuses.New;

                unitOfWork.Repository<TConnectedVehicle>().Insert(vehicleEnt1);
                unitOfWork.Repository<TConnectRequest>().Insert(reqEnt1);
                unitOfWork.Save();

             
                TConnectRequest reqEnt2 = new TConnectRequest();
                reqEnt2.TConnectedVehicleId = vehicleEnt1.Id;
                reqEnt2.TConnectStatusId = (int)TConnectStatuses.New;

                unitOfWork.Repository<TConnectRequest>().Insert(reqEnt2);
                unitOfWork.Save();


                TConnectedVehicleController tvCont = new TConnectedVehicleController(unitOfWork);
                List<TConnVehicleViewModel> vehicles = tvCont.GetVehiclesWithPendingRequests();
                Assert.AreEqual(1, vehicles.Count());
                Assert.AreEqual(2, vehicles[0].NumberRequests, "This bus should have two requests");
            }
        }
        [Test]
        public void ListVehicles_OnlyNew_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                //Setup
                TConnectedVehicle vehicleEnt1 = new TConnectedVehicle();
                vehicleEnt1.Id = 1;
                TConnectRequest reqEnt1 = new TConnectRequest();
                reqEnt1.TConnectedVehicleId = vehicleEnt1.Id;
                reqEnt1.TConnectStatusId = (int)TConnectStatuses.New;

                unitOfWork.Repository<TConnectedVehicle>().Insert(vehicleEnt1);
                unitOfWork.Repository<TConnectRequest>().Insert(reqEnt1);
                unitOfWork.Save();

                TConnectedVehicle vehicleEnt2 = new TConnectedVehicle();
                vehicleEnt2.Id = 2;
                TConnectRequest reqEnt2 = new TConnectRequest();
                reqEnt2.TConnectedVehicleId = vehicleEnt2.Id;
                reqEnt2.TConnectStatusId = (int)TConnectStatuses.Accepted;

                unitOfWork.Repository<TConnectedVehicle>().Insert(vehicleEnt2);
                unitOfWork.Repository<TConnectRequest>().Insert(reqEnt2);
                unitOfWork.Save();


                TConnectedVehicleController tvCont = new TConnectedVehicleController(unitOfWork);
                List<TConnVehicleViewModel> vehicles = tvCont.GetVehiclesWithPendingRequests();
                Assert.AreEqual(1, vehicles.Count());
            }
        }
    }
}
