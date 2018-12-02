using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Zony.Lib.Infrastructures.Middleware;
using Zony.Lib.NetEase;
using System.Threading.Tasks;
using System;
using Zony.Lib.NCMConverter.Convert;

namespace Zony.UITest
{
    [TestClass]
    public class UnitTest1
    {
        public IMiddlewareBudiler Builder { get; set; }

        [TestMethod]
        public void TestMethod1()
        {
            Builder = new MiddlewareBuilder();
            // 管道测试
            Builder.RegisterMiddleware(lyric =>
            {
                return async _ =>
                {
                    _.Append("Method1");
                    await lyric(_);
                };
            });

            Builder.RegisterMiddleware(_ => async __ =>
            {
                __.Append("Method2");
                await _(__);
            });

            var _delegate = Builder.Build();
            var _lyric = new StringBuilder();
            _delegate(_lyric);
            Console.WriteLine("x");

            //var _c = new Startup();
            //var _b = new Startup();
            //_b.DownLoad("Cool Kids", "DJLoveInc", out byte[] data);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var t = new NCMConverter();
            t.ProcessFile(@"D:\Temp\Aimer - Brave Shine.ncm");
        }
    }
}
