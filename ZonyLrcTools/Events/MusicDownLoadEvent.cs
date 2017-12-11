using AutoMapper;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class MusicDownLoadEventData : EventData
    {

    }

    public class MusicDownLoadEvent : IEventHandler<MusicDownLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_pluginManager;

        public MusicDownLoadEvent(IPluginManager pluginManager)
        {
            m_pluginManager = pluginManager;
        }

        public async void HandleEvent(MusicDownLoadEventData eventData)
        {
            IPluginDownLoader _downloader = m_pluginManager.GetPlugin<IPluginDownLoader>();

            await Task.Run(() =>
            {
                Parallel.ForEach(GlobalContext.Instance.MusicInfos, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, (info) =>
                    {
                        try
                        {
                            _downloader.DownLoad(info.Song, info.Artist, out byte[] _lyricData);
                            if (_lyricData == null)
                            {
                                GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[info.Index].SubItems[4].Text = AppConsts.Status_Music_Failed;
                                return;
                            }

                            // 写入歌词
                            var _eventData = Mapper.Map<MusicDownLoadCompleteEventData>(eventData);
                            _eventData.LyricData = _lyricData;
                            _eventData.Info = info;

                            EventBus.Default.Trigger(_eventData);
                        }
                        catch (NotFoundLyricException)
                        {
                            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[info.Index].SubItems[4].Text = AppConsts.Status_Music_NotFoundLyric;
                        }
                    });
            });
        }
    }
}