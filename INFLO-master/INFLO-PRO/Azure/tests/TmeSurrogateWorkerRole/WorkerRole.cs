/*!
    @file         TmeSurrogateWorkerRole/WorkerRole.cs
    @author       Luke Kucalaba

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace TmeSurrogateWorkerRole
{
    public class WorkerRole : Microsoft.WindowsAzure.ServiceRuntime.RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("TmeSurrogateWorkerRole entry point called", "Information");

            while(true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;  //maximum number of concurrent connections 

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
