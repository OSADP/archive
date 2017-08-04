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
using IDTO.DataProcessor.VehicleLocationMonitor;
using IDTO.BusScheduleInterface;
using IDTO.BusScheduleInterface.Fakes;

namespace IDTO.UnitTests.IDTO.DataProcessor
{
    [TestFixture]
    public class VehicleLocationMonitorWorkerTest
    {

        //IBusSchedule busScheduleFake = new StubIBusSchedule();
        List<IBusSchedule> scheduleFakes = new List<IBusSchedule>(new IBusSchedule[]{new StubIBusSchedule()});

        [Test]
        public void ResolveVehicleLocationProviderType_CapTrans_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {

                Step stepEntity = new Step();
                stepEntity.StepNumber = 1;
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

                unitOfWork.Repository<Step>().Insert(stepEntity);
                unitOfWork.Save();

                TConnect tconn = new TConnect();
                tconn.InboundStepId = stepEntity.Id;

                unitOfWork.Repository<TConnect>().Insert(tconn);
                unitOfWork.Save();


                IVehicleLocation iv = VehicleLocationMonitorWorker.ResolveVehicleLocationProviderType(tconn, unitOfWork, scheduleFakes);
                Assert.AreEqual(iv.ProviderName, Providers.CapTrans);

            }
        }

        [Test]
        public void ResolveVehicleLocationProviderType_Cabs_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {

                Step stepEntity = new Step();
                stepEntity.StepNumber = 1;
                stepEntity.StartDate = DateTime.Parse("1/1/2014 10:02");
                stepEntity.EndDate = DateTime.Parse("1/1/2014 10:40");
                stepEntity.FromName = "Quartz Street";
                stepEntity.FromProviderId = (int)Providers.CABS;
                stepEntity.FromStopCode = "1001";
                stepEntity.ModeId = (int)Modes.Bus;
                stepEntity.RouteNumber = "039";
                stepEntity.Distance = (decimal)12.2;
                stepEntity.ToName = "Slate Run Road";
                stepEntity.ToProviderId = (int)Providers.CABS;
                stepEntity.ToStopCode = "2002";

                unitOfWork.Repository<Step>().Insert(stepEntity);
                unitOfWork.Save();

                TConnect tconn = new TConnect();
                tconn.InboundStepId = stepEntity.Id;

                unitOfWork.Repository<TConnect>().Insert(tconn);
                unitOfWork.Save();

                IVehicleLocation iv = VehicleLocationMonitorWorker.ResolveVehicleLocationProviderType(tconn, unitOfWork, scheduleFakes);
                Assert.AreEqual(iv.ProviderName, Providers.CABS);

            }
        }
        [Test]
        public void ResolveVehicleLocationProviderType_Other_Fails()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {

                Step stepEntity = new Step();
                stepEntity.StepNumber = 1;
                stepEntity.StartDate = DateTime.Parse("1/1/2014 10:02");
                stepEntity.EndDate = DateTime.Parse("1/1/2014 10:40");
                stepEntity.FromName = "Quartz Street";
                stepEntity.FromProviderId = (int)Providers.COTA;
                stepEntity.FromStopCode = "1001";
                stepEntity.ModeId = (int)Modes.Bus;
                stepEntity.RouteNumber = "039";
                stepEntity.Distance = (decimal)12.2;
                stepEntity.ToName = "Slate Run Road";
                stepEntity.ToProviderId = (int)Providers.COTA;
                stepEntity.ToStopCode = "2002";

                unitOfWork.Repository<Step>().Insert(stepEntity);
                unitOfWork.Save();

                TConnect tconn = new TConnect();
                tconn.InboundStepId = stepEntity.Id;

                unitOfWork.Repository<TConnect>().Insert(tconn);
                unitOfWork.Save();
                try
                {

                    IVehicleLocation iv = VehicleLocationMonitorWorker.ResolveVehicleLocationProviderType(tconn, unitOfWork, scheduleFakes);
                    Assert.AreEqual(true, false, "Test should have thrown an exception and not executed this line.");

                }
                catch (Exception ex)
                {
                    //unhandled type
                }

            }
        }
    }
}
