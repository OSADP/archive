using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.DataProcessor.Common;

namespace IDTO.DataProcessor.VehicleLocationMonitor
{
    public class VehicleLocationMonitorManager : BaseProcManager
    {
        public VehicleLocationMonitorManager() : base(new VehicleLocationMonitorWorker())
        {}

    }
}
