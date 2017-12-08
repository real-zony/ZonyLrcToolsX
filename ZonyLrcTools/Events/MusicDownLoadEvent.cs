using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class MusicDownLoadEventData : MainUIComponentContext
    {

    }

    public class MusicDownLoadEvent : IEventHandler<MusicDownLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_pluginManager;
        private readonly GlobalContext m_globalContext;

        public MusicDownLoadEvent(IPluginManager pluginManager, GlobalContext globalContext)
        {
            m_pluginManager = pluginManager;
            m_globalContext = globalContext;
        }

        public async void HandleEvent(MusicDownLoadEventData eventData)
        {
            IPluginDownLoader _downloader = m_pluginManager.GetPlugin<IPluginDownLoader>();

            await Task.Run(() =>
            {
                Parallel.ForEach(m_globalContext.MusicInfos, (info) =>
                {
                    _downloader.DownLoad(info.Song, info.Artist, out byte[] _lyricData);
                    if (_lyricData == null)
                    {
                        eventData.Center_ListViewNF_MusicList.Items[info.Index].SubItems[4].Text = AppConsts.Status_Music_Success;
                        return;
                    }
                    EventBus.Default.Trigger(eventData as MainUIComponentContext as MusicDownLoadCompleteEventData);
                });
            });
        }
    }
}
