using System.Collections.Concurrent;
using Zony.Lib.Plugin.Models;

namespace ZonyLrcTools.Common
{
    public class GlobalContext
    {
        private static GlobalContext m_uniqueInstance;
        private static readonly object m_locker = new object();

        private GlobalContext() { }

        public static GlobalContext Instance
        {
            get
            {
                if (m_uniqueInstance == null)
                {
                    lock (m_locker)
                    {
                        if (m_uniqueInstance == null) m_uniqueInstance = new GlobalContext();
                    }
                }

                return m_uniqueInstance;
            }
        }

        public ConcurrentBag<MusicInfoModel> MusicInfos { get; set; }

        public MainUIComponentContext UIContext { get; set; }
    }
}