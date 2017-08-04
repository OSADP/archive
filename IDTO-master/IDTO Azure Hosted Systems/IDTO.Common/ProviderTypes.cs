using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.Common
{
    public enum ProviderTypes
    {
        //Must match DB providertypes Table
        FixedRoute_TConnect = 1,
        IncomingFixedRoute = 2,
        Rideshare=3,
        Demand_Response=4
    }
}
