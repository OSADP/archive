using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using IDTO.Service;
using IDTO.Common;
using Moq;
using NUnit.Framework;
using IDTO.DataProcessor.TravelerMonitor;

using IDTO.DataProcessor;
namespace IDTO.UnitTests.IDTO.DataProcessor
{
    [TestFixture]
    class TravelerMonitorWorkerTest : TravelerMonitorWorker
    {
        [Test]
        public void testSendEmail_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOContext(@"Server=tcp:sfkee7y99k.database.windows.net,1433;Database=IDTO_Test;User ID=vital_admin@sfkee7y99k;Password=Super$0813^Monkey;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"))
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TravelerMonitorWorker worker = new TravelerMonitorWorker();
                worker.SendInactivityEmails(unitOfWork);
            
            }
        }
    }  
}
