using System.IO;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.UIComponents;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class SingleMusicInfoLoadEventData : EventData
    {
        public ListViewNF MusicListView { get; set; }
    }

    public class SingleMusicInfoLoadEvent : IEventHandler<SingleMusicInfoLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_plugManager;
        private readonly GlobalContext m_globalContext;

        public SingleMusicInfoLoadEvent(IPluginManager plugManager, GlobalContext globalContext)
        {
            m_plugManager = plugManager;
            m_globalContext = globalContext;
        }

        public void HandleEvent(SingleMusicInfoLoadEventData eventData)
        {
            ListViewNF _listView = eventData.MusicListView;

            if (_listView.SelectedItems == null) return;

            IPluginAcquireMusicInfo _acquire = m_plugManager.GetPlugin<IPluginAcquireMusicInfo>();
            Stream _imgStream = _acquire.LoadAlbumImage(m_globalContext.MusicInfos[0].FilePath);
            

        }
    }
}
