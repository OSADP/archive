using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public enum LogEventTypes
    {
        //These mus match the DB Table of same name

        DGPS_CONFIG_INIT = 1,
        BLUETOOTH_CONFIG_INIT,
        CAPWIN_CONFIG_INIT,
        DSRC_CONFIG_INIT,
        DGPS_CONFIG,
        BLUETOOTH_CONFIG,
        CAPWIN_CONFIG,
        DSRC_CONFIG,
        SYSTEM_ERROR,
        CAPWIN_CONNECTING,
        CAPWIN_UNKNOWN,
        CAPWIN_DISCONNECTED,
        CAPWIN_CONNECTED,
        DGPS_CONNECTING,
        DGPS_UNKNOWN,
        DGPS_DISCONNECTED,
        DGPS_CONNECTED,
        CAPWIN_MOBILE_CONNECTING,
        CAPWIN_MOBILE_UNKNOWN,
        CAPWIN_MOBILE_DISCONNECTED,
        CAPWIN_MOBILE_CONNECTED,
        BLUETOOTH_CONNECTING,
        BLUETOOTH_Unknown,
        BLUETOOTH_DISCONNECTED,
        BLUETOOTH_CONNECTED,
        ALARM_CONFIG_INITIAL,
        ALARM_CONFIG,
        TIM,
        EVA,
        THREAT,
        INCIDENT
    };
}
