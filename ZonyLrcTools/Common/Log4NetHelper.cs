using log4net;
using System;
using System.IO;
using Zony.Lib.Plugin.Common;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ZonyLrcTools.Common
{
    public static class Log4NetHelper
    {
        private static ILog _log;
        private static ILog Log
        {
            get
            {
                string configPath = Environment.CurrentDirectory + "\\log4net.config";
                log4net.Config.XmlConfigurator.Configure(new FileInfo(configPath));

                if (_log == null) _log = LogManager.GetLogger(AppConsts.AppName);
                return _log;
            }
        }

        public static void Exception(Exception e)
        {
            Log.Error(e);
        }

        public static void Info(string message)
        {
            Log.Info(message);
        }
    }
}
