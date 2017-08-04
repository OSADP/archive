using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.DataProcessor.Common;
using IDTO.BusScheduleInterface;

namespace IDTO.DataProcessor.TConnectMonitor
{
    public class TConnectMonitorManager : BaseProcManager
    {
        public TConnectMonitorManager(List<IBusSchedule> busSchedulerAPIs)
            : base(new TConnectMonitorWorker(busSchedulerAPIs))
        {}

    }
}
