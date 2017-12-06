using System.Collections.Generic;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Interfaces;

namespace ZonyLrcTools.Events
{
    public class MusicInfoLoadEventData : EventData
    {
        public Dictionary<string, List<string>> MusicFilePaths { get; set; }
    }

    public class MusicInfoLoadEvent : IEventHandler<MusicInfoLoadEventData>
    {
        private readonly IPluginAcquireMusicInfo m_infoLoader;
        public MusicInfoLoadEvent(IPluginAcquireMusicInfo infoLoader)
        {
            m_infoLoader = infoLoader;
        }

        public async void HandleEvent(MusicInfoLoadEventData eventData)
        {
            var _result = await m_infoLoader.GetMusicInfosAsync(eventData.MusicFilePaths);

        }
    }
}
