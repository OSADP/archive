using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.DataProcessor.Common;
using IDTO.Common;
using Ninject;
using IDTO.Entity.Models;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.BusScheduleInterface;
namespace IDTO.DataProcessor.VehicleLocationMonitor
{
    public class VehicleLocationMonitorWorker : BaseProcWorker
    {
        protected IDbContext db;//= new IDTOContext();
        public IUnitOfWork Uow;
        public override void PerformWork()
        {

        //Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral, 
       //         DateTime.UtcNow.ToLongTimeString() + " - Entering PerformWork for VehicleLocationMonitorWorker");

            IDbContext db = WorkerRole.Kernel.Get<IDbContext>();


            Uow = new UnitOfWork(db);


            UpdateEstimatedArrivals(Uow);
        }

        private void UpdateEstimatedArrivals(IUnitOfWork Uow)
        {

            //List<TConnect> monitoredTConnects = Uow.Repository<TConnect>().Query().Get()
            //           .Where(s => s.TConnectStatusId == (int)TConnectStatuses.Monitored).ToList();

            //foreach (TConnect mt in monitoredTConnects)
            //{
            //    IVehicleLocation vehLoc;
            //    vehLoc = ResolveVehicleLocationProviderType(mt);
            //    DateTime Eta = vehLoc.CalculateEstimatedTimeOfArrival(mt, Uow);
            //    //TODO, do we want to add a ETA column to database Tconnect table?

            //}
        }

        /// <summary>
        /// Based on the TConnect's Inbound Step Provider, determine if the appropriate
        /// type of vehicle location provider to use
        /// Uses TOprovider
        /// </summary>
        /// <param name="mt"></param>
        /// <returns></returns>
        public static IVehicleLocation ResolveVehicleLocationProviderType(TConnect mt, IUnitOfWork Uow, List<IBusSchedule> busScheduleAPIs)
        {
            IVehicleLocation vehLoc= null;
            Step inboundStep = Uow.Repository<Step>().Query().Get()
                    .Where(s=>s.Id.Equals(mt.InboundStepId)).First();
            if (inboundStep.ToProviderId == (int)Providers.CapTrans)
            {
                vehLoc = new CapTransVehicleLocation();

            }
            else
            {
                foreach (IBusSchedule busSchedule in busScheduleAPIs)
                {
                    if(inboundStep.ToProviderId == (int)busSchedule.GetProviderId())
                    {
                        vehLoc = new CabsVehicleLocation(busSchedule);
                    }
                }
            }
            
            if(vehLoc== null)
            {
                throw new NotImplementedException("Tconnects only handled from CapTrans and CABS currently. ");
            }
            return vehLoc;
        }
    }
}
