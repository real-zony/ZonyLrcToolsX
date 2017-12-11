using log4net;
using System;
using System.IO;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ZonyLrcTools.Common
{
    public static class Log4NetHelper
    {
        private static ILog m_log;
        private static ILog Log
        {
            get
            {
                string _configPath = Environment.CurrentDirectory + "\\log4net.config";
                log4net.Config.XmlConfigurator.Configure(new FileInfo(_configPath));

                if (m_log == null) m_log = LogManager.GetLogger(AppConsts.AppName);
                return m_log;
            }
        }

        public static void Exception(Exception E)
        {
            Log.Error(E);
        }

        public static void Info(string message)
        {
            Log.Info(message);
        }
    }
}
