using System.Collections.Concurrent;
using System.Collections.Generic;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Common
{
    public class GlobalContext
    {
        private static GlobalContext m_uniqueInstance;
        private static readonly object m_locker = new object();

        private GlobalContext() { }

        public bool LyricDownloadState { get; set; }

        public bool AlbumDownloadState { get; set; }

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

        public List<MusicInfoModel> MusicInfos { get; set; }

        public MainUIComponentContext UIContext { get; set; }

        public ConcurrentBag<MusicInfoModel> GetConcurrentList()
        {
            ConcurrentBag<MusicInfoModel> _infos = new ConcurrentBag<MusicInfoModel>();

            foreach (var _item in MusicInfos)
            {
                _infos.Add(_item);
            }

            return _infos;
        }
    }
}
