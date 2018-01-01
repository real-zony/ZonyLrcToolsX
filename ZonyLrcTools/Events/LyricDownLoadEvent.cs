using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Enums;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Interfaces;
using ZonyLrcTools.Events.UIEvents;

namespace ZonyLrcTools.Events
{
    public class LyricDownLoadEventData : EventData
    {
        /// <summary>
        /// 要下载的音乐集合
        /// </summary>
        public IEnumerable<MusicInfoModel> MusicInfos { get; set; }

        public LyricDownLoadEventData(IEnumerable<MusicInfoModel> musicInfos)
        {
            MusicInfos = musicInfos;
        }
    }

    public class LyricDownLoadEvent : IEventHandler<LyricDownLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_pluginManager;
        private readonly IConfigurationManager m_configMgr;

        public LyricDownLoadEvent(IPluginManager pluginManager, IConfigurationManager configMgr)
        {
            m_pluginManager = pluginManager;
            m_configMgr = configMgr;
        }

        public async void HandleEvent(LyricDownLoadEventData eventData)
        {
            if (GlobalContext.Instance.MusicInfos.Count == 0 || GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items.Count == 0) MessageBox.Show("你还没有添加歌曲文件!", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);

            IPluginDownLoader _downloader = m_pluginManager.GetPlugin<IPluginDownLoader>();

            await Task.Run(() =>
            {
                Parallel.ForEach(eventData.MusicInfos, new ParallelOptions() { MaxDegreeOfParallelism = m_configMgr.ConfigModel.DownloadThreadNumber }, (info, loopState) =>
                    {
                        try
                        {
                            // 略过歌词
                            if (!m_configMgr.ConfigModel.IsReplaceLyricFile)
                            {
                                info.Status = MusicInfoEnum.Igonre;
                                return;
                            }

                            _downloader.DownLoad(info.Song, info.Artist, out byte[] _lyricData);

                            if (_lyricData == null)
                            {
                                GlobalContext.Instance.SetItemStatus(info.Index, AppConsts.Status_Music_Failed);
                                info.Status = MusicInfoEnum.Failed;
                                return;
                            }

                            info.Status = MusicInfoEnum.Success;
                            EventBus.Default.Trigger(new LyricDownLoadCompleteEventData()
                            {
                                LyricData = _lyricData,
                                Info = info
                            });
                        }
                        catch (NotFoundLyricException)
                        {
                            info.Status = MusicInfoEnum.NotFound;
                            GlobalContext.Instance.SetItemStatus(info.Index, AppConsts.Status_Music_NotFoundLyric);
                        }
                        finally
                        {
                            GlobalContext.Instance.SetBottomStatusText($"{AppConsts.Status_Bottom_DownLoadHead}{info.Song}");
                            GlobalContext.Instance.UIContext.Bottom_ProgressBar.Value++;
                            if (GlobalContext.Instance.UIContext.Bottom_ProgressBar.Value == GlobalContext.Instance.UIContext.Bottom_ProgressBar.Maximum) EventBus.Default.Trigger<UIComponentDownloadCompleteEventData>();
                        }
                    });
            });
        }
    }
}