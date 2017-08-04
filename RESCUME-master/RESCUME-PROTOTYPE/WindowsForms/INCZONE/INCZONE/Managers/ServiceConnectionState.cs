using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Managers
{
    public enum ServiceConnectionState
    {
        Unknown = 1,
        Disconnected = 2,
        Connecting = 3,
        Connected = 4,
        Bypassed = 5
    }
}
