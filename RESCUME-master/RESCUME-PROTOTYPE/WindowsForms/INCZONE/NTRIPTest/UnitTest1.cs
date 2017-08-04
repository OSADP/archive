using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NTRIP;

namespace NTRIPTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            string socket = NTRIPConnection.DoSocketGet("156.63.133.111", 2101, "battelle2", "rtkPass");
        }

        [TestMethod]
        public void TestMethod2()
        {

            NTRIPConfiguration.StoreConfiguration("156.63.133.118", 2101, "battelle2", "rtkPass");
        }
    }
}
