using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Ninject.MockingKernel.Moq;

namespace Tests
{
    public class TestBase
    {
        public MoqMockingKernel Kernel { get; set; }

        [SetUp]
        public void BaseSetUp()
        {
            Kernel = new MoqMockingKernel();
            MockData.Kernel = Kernel;
        }
    }
}
