using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Http.Hosting;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using Moq;
using NUnit.Framework;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using IDTO.Common;

namespace IDTO.UnitTests.IDTO.WebAPI
{

    [TestFixture]
    public class TConnectControllerTest
    {

        [Test]
        public void NewTConnect_NotStarted_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Trip t = new Trip { Id = 1, TripStartDate = DateTime.UtcNow.AddHours(5) };
                unitOfWork.Repository<Trip>().Insert(t);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                var status = controller.Get(t.Id).First();
                Assert.AreEqual((int)TConnectStatusModel.Status.Saved, status.TConnectStatusId);
            }
        }
        [Test]
        public void TConnect_Completed_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Trip t = new Trip { Id = 1, TripStartDate = DateTime.UtcNow.AddHours(-5), TripEndDate = DateTime.UtcNow.AddHours(-4) };
                unitOfWork.Repository<Trip>().Insert(t);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                var status = controller.Get(t.Id).First();
                Assert.AreEqual((int)TConnectStatusModel.Status.Completed, status.TConnectStatusId);
            }
        }

        [Test]
        public void TConnect_InProgress_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                Trip t = new Trip { Id = 1, TripStartDate = DateTime.UtcNow.AddHours(-1), TripEndDate = DateTime.UtcNow.AddHours(1) };
                unitOfWork.Repository<Trip>().Insert(t);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                var status = controller.Get(t.Id).First();
                Assert.AreEqual((int)TConnectStatusModel.Status.InProgress, status.TConnectStatusId);
            }
        }
        [Test]
        public void TConnect_NoRequest_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.New };
                unitOfWork.Repository<TConnect>().Insert(tc);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                int status = (int)controller.DeduceExternalStatusForTConnect(tc);
                Assert.AreEqual((int)TConnectStatusModel.Status.Monitored, status);
            }
        }
     
   
        [Test]
        public void DoneTConnect_NewRequest_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.Done };
                unitOfWork.Repository<TConnect>().Insert(tc);
                TConnectRequest tr = new TConnectRequest { TConnectId = tc.Id, TConnectStatusId = (int)TConnectStatuses.New };
                unitOfWork.Repository<TConnectRequest>().Insert(tr); 
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                int status = (int)controller.DeduceExternalStatusForTConnect(tc);
                Assert.AreEqual((int)TConnectStatusModel.Status.AutoRejected, status);
            }
        }
        [Test]
        public void DoneTConnect_AcceptedRequest_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.Done };
                unitOfWork.Repository<TConnect>().Insert(tc);
                TConnectRequest tr = new TConnectRequest { TConnectId = tc.Id, TConnectStatusId = (int)TConnectStatuses.Accepted };
                unitOfWork.Repository<TConnectRequest>().Insert(tr);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                int status = (int)controller.DeduceExternalStatusForTConnect(tc);
                Assert.AreEqual((int)TConnectStatusModel.Status.Accepted, status);
            }
        }
        public void DoneTConnect_RejectedRequest_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.Done };
                unitOfWork.Repository<TConnect>().Insert(tc);
                TConnectRequest tr = new TConnectRequest { TConnectId = tc.Id, TConnectStatusId = (int)TConnectStatuses.Rejected };
                unitOfWork.Repository<TConnectRequest>().Insert(tr);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                int status = (int)controller.DeduceExternalStatusForTConnect(tc);
                Assert.AreEqual((int)TConnectStatusModel.Status.Rejected, status);
            }
        }
        
        [Test]
        public void MonitoredTConnect_AcceptedRequest_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.Monitored };
                unitOfWork.Repository<TConnect>().Insert(tc);
                TConnectRequest tr = new TConnectRequest { TConnectId = tc.Id, TConnectStatusId = (int)TConnectStatuses.Accepted };
                unitOfWork.Repository<TConnectRequest>().Insert(tr);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                int status = (int)controller.DeduceExternalStatusForTConnect(tc);
                Assert.AreEqual((int)TConnectStatusModel.Status.Accepted, status);
            }
        }
        [Test]
        public void MonitoredTConnect_RejectedRequest_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.Monitored };
                unitOfWork.Repository<TConnect>().Insert(tc);
                TConnectRequest tr = new TConnectRequest { TConnectId = tc.Id, TConnectStatusId = (int)TConnectStatuses.Rejected };
                unitOfWork.Repository<TConnectRequest>().Insert(tr);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                int status = (int)controller.DeduceExternalStatusForTConnect(tc);
                Assert.AreEqual((int)TConnectStatusModel.Status.Rejected, status);
            }
        }
        [Test]
        public void MonitoredTConnect_NewRequest_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                TConnect tc = new TConnect { Id = 1, TConnectStatusId = (int)TConnectStatuses.Monitored };
                unitOfWork.Repository<TConnect>().Insert(tc);
                TConnectRequest tr = new TConnectRequest { TConnectId = tc.Id, TConnectStatusId = (int)TConnectStatuses.New };
                unitOfWork.Repository<TConnectRequest>().Insert(tr);
                unitOfWork.Save();

                var controller = new TConnectController(idtoFakeContext);
                int status = (int)controller.DeduceExternalStatusForTConnect(tc);
                Assert.AreEqual((int)TConnectStatusModel.Status.Requested, status);
            }
        }

        [Test]
        public void GetTConnectRequestsTest()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                var requests = unitOfWork.Repository<TConnectRequest>().Query().Get().ToList();

                Assert.IsTrue(requests.Count > 0);
            }
        }


    }
}
