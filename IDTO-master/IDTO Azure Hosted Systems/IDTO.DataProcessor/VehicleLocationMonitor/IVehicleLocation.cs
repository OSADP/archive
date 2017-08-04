using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Common;
using Repository;
namespace IDTO.DataProcessor.VehicleLocationMonitor
{
    public interface IVehicleLocation
    {
        Providers ProviderName {get;}
         Task<DateTime> CalculateEstimatedTimeOfArrivalAsync(TConnect tConnect, IUnitOfWork Uow);
         string GetInboundVehicleName(TConnect tConnect, IUnitOfWork Uow);
    }
}
