using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.DataProcessor.TConnectMonitor
{
    class LocationMonitoringNotificationSwitchboard
    {
        public bool SentStartInboundStep { get; set; }
        public bool SentEndInboundStep { get; set; }
        public bool SentStartOutboundStep { get; set; }
        public bool SentTConnectAccept { get; set; }
        public bool SentTConnectReject { get; set; }
        public LocationMonitoringNotificationSwitchboard()
        {
            SentStartInboundStep = false;
            SentEndInboundStep = false;
            SentStartOutboundStep = false;
            SentTConnectAccept = false;
            SentTConnectReject = false;
        }
    }
}
