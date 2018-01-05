using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zony.Lib.NetEase;

namespace Zony.UITest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var _downloader = new Startup();
            _downloader.DownLoad("The Wolven Storm", "Priscilla", out byte[] data);
            //var _c = new Startup();
            //var _b = new Startup();
            //_b.DownLoad("Cool Kids", "DJLoveInc", out byte[] data);
        }
    }
}
