using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zony.Lib.AlbumDownLoad;
using Zony.Lib.Plugin.Models;

namespace Zony.UITest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var _c = new Startup();
            _c.DownlaodAblumImage(new MusicInfoModel() { Artist = "DJLoveInc", Song = "Cool Kids" }, out byte[] data);
            //var _b = new Startup();
            //_b.DownLoad("Cool Kids", "DJLoveInc", out byte[] data);
        }
    }
}
