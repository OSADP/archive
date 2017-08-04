using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.DataProcessor.Common;

namespace IDTO.DataProcessor.TravelerMonitor
{
    class TravelerMonitorManager : BaseProcManager
    {
        public TravelerMonitorManager() : base(new TravelerMonitorWorker())
        {}
    }
}
