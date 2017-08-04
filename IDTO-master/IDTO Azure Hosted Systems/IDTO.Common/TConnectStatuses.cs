using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.Common
{
    /// <summary>
    /// Must match DB TConnectStatus Table. Used for TConnect table and TConnectRequestTable
    /// </summary>
    public enum TConnectStatuses
    {
        //
        New = 1,
        Monitored = 2,
        Done=3,
        Accepted=4,
        Rejected=5
    }
}
