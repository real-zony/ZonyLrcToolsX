using System.Collections.Concurrent;
using System.Collections.Generic;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Common
{
    /// <summary>
    /// 全局上下文
    /// </summary>
    public class GlobalContext
    {
        private static GlobalContext m_uniqueInstance;
        private static readonly object m_locker = new object();

        private GlobalContext()
        {
            MusicInfos = new List<MusicInfoModel>();
        }

        /// <summary>
        /// 歌词下载状态
        /// </summary>
        public bool LyricDownloadState { get; set; }

        /// <summary>
        /// 专辑图像下载状态
        /// </summary>
        public bool AlbumDownloadState { get; set; }

        /// <summary>
        /// 单例对象
        /// </summary>
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

        /// <summary>
        /// 目前所加载的所有音乐信息
        /// </summary>
        public List<MusicInfoModel> MusicInfos { get; set; }

        /// <summary>
        /// 主程序开放的所有 UI 组件
        /// </summary>
        public MainUIComponentContext UIContext { get; set; }

        /// <summary>
        /// 将当前音乐信息转换为 ConcurrentBag 容器
        /// 注:会产生新的对象
        /// </summary>
        /// <returns>转换完成的包含所有音乐信息的 ConcurrentBag 实例</returns>
        public ConcurrentBag<MusicInfoModel> GetConcurrentList()
        {
            ConcurrentBag<MusicInfoModel> _infos = new ConcurrentBag<MusicInfoModel>();
            if (MusicInfos == null) MusicInfos = new List<MusicInfoModel>();

            foreach (var _item in MusicInfos)
            {
                _infos.Add(_item);
            }

            return _infos;
        }

        /// <summary>
        /// 一个安全添加歌曲信息的方法
        /// </summary>
        /// <param name="infos">需要添加的歌曲信息</param>
        public void AddMusicInfosAndFillListView(IEnumerable<MusicInfoModel> infos)
        {
            lock (MusicInfos)
            {
                int _lastIndex = MusicInfos[MusicInfos.Count - 1].Index;
                foreach (var _info in infos)
                {
                    _info.Index = ++_lastIndex;
                }
                MusicInfos.AddRange(infos);
            }
        }

        /// <summary>
        /// 一个安全添加歌曲信息的方法
        /// </summary>
        /// <param name="infos">需要添加的歌曲信息</param>
        public void AddMusicInfoAndFillListView(MusicInfoModel info)
        {
            lock (MusicInfos)
            {
                int _lastIndex = MusicInfos[MusicInfos.Count - 1].Index;
                info.Index = ++_lastIndex;
                MusicInfos.Add(info);
            }
        }
    }
}
