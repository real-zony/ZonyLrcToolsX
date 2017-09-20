using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zony.Lib.NetEase.Plugin;

namespace Zony.UITest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var _b = new Startup();
            _b.DownLoad("[A]ddiction", "EVO+", out byte[] data);
        }
    }
}
