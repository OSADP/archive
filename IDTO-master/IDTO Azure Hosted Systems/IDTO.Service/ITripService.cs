using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using Repository;
namespace IDTO.Service
{
    public interface ITripService
    {
        int SaveTrip(Trip trip, List<Step> steps, IUnitOfWork Uow);
    }
}
