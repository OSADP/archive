using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Data;
using IDTO.Entity.Models;
using IDTO.UnitTests.Fake;
using Moq;
using NUnit.Framework;
using Repository;
using Repository.Providers.EntityFramework;

namespace IDTO.UnitTests.IDTO.Data
{
    [TestFixture]
    public class IDTOContextTest
    {
        [Test]
        public void BasicRepositoryInsertDeleteAndQueryTest_Passes()
        {

            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                unitOfWork.Repository<Traveler>().Insert(new Traveler { Id = 1, FirstName = "TestFN", LastName = "TestLN", ObjectState = ObjectState.Added });

                unitOfWork.Save();
                //Test

                var traveler1 = unitOfWork.Repository<Traveler>().Find(1);

                Assert.AreEqual("TestFN", traveler1.FirstName);

                unitOfWork.Repository<Traveler>().Delete(1);

                unitOfWork.Save();

                var traveler2 = unitOfWork.Repository<Traveler>().Find(1);


                Assert.IsNull(traveler2);
            }

        }
    }
}
