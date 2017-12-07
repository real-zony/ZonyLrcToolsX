using System.Collections.Generic;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;

namespace ZonyLrcTools.Events
{
    public class MusicInfoLoadEventData : EventData
    {
        public Dictionary<string, List<string>> MusicFilePaths { get; set; }
    }

    public class MusicInfoLoadEvent : IEventHandler<MusicInfoLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_plugMgr;
        public MusicInfoLoadEvent(IPluginManager plugMgr)
        {
            m_plugMgr = plugMgr;
        }

        public void HandleEvent(MusicInfoLoadEventData eventData)
        {
            m_plugMgr.LoadPlugins();
            var _acquire = m_plugMgr.GetPlugin<IPluginAcquireMusicInfo>();
            var _infos = _acquire.GetMusicInfos(eventData.MusicFilePaths);
        }
    }
}
