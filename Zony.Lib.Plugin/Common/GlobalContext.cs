using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Zony.Lib.Plugin.Enums;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Common
{
    /// <summary>
    /// 全局上下文
    /// </summary>
    public class GlobalContext
    {
        private static GlobalContext _uniqueInstance;

        private static readonly object Locker = new object();

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
                if (_uniqueInstance == null)
                {
                    lock (Locker)
                    {
                        if (_uniqueInstance == null) _uniqueInstance = new GlobalContext();
                    }
                }

                return _uniqueInstance;
            }
        }

        /// <summary>
        /// 目前所加载的所有音乐信息
        /// </summary>
        public List<MusicInfoModel> MusicInfos { get; set; }

        /// <summary>
        /// 主程序开放的所有 UI 组件
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public MainUIComponentContext UIContext { get; set; }

        /// <summary>
        /// 将当前音乐信息转换为 ConcurrentBag 容器
        /// 注:会产生新的对象
        /// </summary>
        /// <returns>转换完成的包含所有音乐信息的 ConcurrentBag 实例</returns>
        public ConcurrentBag<MusicInfoModel> GetConcurrentList()
        {
            ConcurrentBag<MusicInfoModel> infos = new ConcurrentBag<MusicInfoModel>();
            if (MusicInfos == null) MusicInfos = new List<MusicInfoModel>();

            foreach (var item in MusicInfos.OrderBy(z => z.Index))
            {
                infos.Add(item);
            }

            return infos;
        }

        /// <summary>
        /// 添加一个歌曲信息集合到全局容器
        /// </summary>
        /// <param name="infos">歌曲信息集合</param>
        public void InsertMusicInfos(IEnumerable<MusicInfoModel> infos)
        {
            lock (MusicInfos)
            {
                int lastIndex = MusicInfos.Count == 0 ? 0 : MusicInfos[MusicInfos.Count - 1].Index;
                foreach (var info in infos)
                {
                    info.Status = MusicInfoEnum.Ready;
                    info.Index = lastIndex++;
                }
            }
        }

        /// <summary>
        /// 添加一个歌曲信息到全局容器
        /// </summary>
        /// <param name="info">歌曲信息</param>
        public void InsertMusicInfo(MusicInfoModel info)
        {
            lock (MusicInfos)
            {
                int lastIndex = MusicInfos.Count == 0 ? 0 : MusicInfos[MusicInfos.Count - 1].Index;
                info.Status = MusicInfoEnum.Ready;
                info.Index = ++lastIndex;
                MusicInfos.Add(info);
            }
        }

        /// <summary>
        /// 一个安全添加歌曲信息的方法，并且填充 UI
        /// </summary>
        /// <param name="infos">需要添加的歌曲信息集合</param>
        public void InsertMusicInfosAndFillListView(IEnumerable<MusicInfoModel> infos)
        {
            lock (MusicInfos)
            {
                int lastIndex = MusicInfos.Count == 0 ? 0 : MusicInfos[MusicInfos.Count - 1].Index;
                UIContext.Center_ListViewNF_MusicList.BeginUpdate();
                foreach (var info in infos)
                {
                    info.Status = MusicInfoEnum.Ready;
                    info.Index = ++lastIndex;
                    InsertItemToCenterListView(info);
                }
                UIContext.Center_ListViewNF_MusicList.EndUpdate();
                MusicInfos.AddRange(infos);
            }
        }

        /// <summary>
        /// 一个安全添加歌曲信息的方法，并且填充 UI
        /// </summary>
        /// <param name="info">需要添加的歌曲信息</param>
        public void InsertMusicInfoAndFillListView(MusicInfoModel info)
        {
            lock (MusicInfos)
            {
                int lastIndex = MusicInfos.Count == 0 ? 0 : MusicInfos[MusicInfos.Count - 1].Index;
                info.Status = MusicInfoEnum.Ready;
                info.Index = ++lastIndex;
                MusicInfos.Add(info);
                InsertItemToCenterListView(info);
            }
        }

        /// <summary>
        /// 向 CenterListView 当中添加条目
        /// </summary>
        /// <param name="info">要添加的音乐信息</param>
        public void InsertItemToCenterListView(MusicInfoModel info)
        {
            UIContext.Center_ListViewNF_MusicList.Items.Insert(info.Index, new ListViewItem(new[]
                {
                    info.Song,
                    info.Artist,
                    info.Album,
                    info.TagType,
                    "等待下载"
                }));
        }
    }
}
