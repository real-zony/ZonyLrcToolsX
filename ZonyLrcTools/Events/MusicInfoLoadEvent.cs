using System.Collections.Generic;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;

namespace ZonyLrcTools.Events
{
    public class MusicInfoLoadEventData : EventData
    {
        public Dictionary<string, List<string>> MusicFilePaths { get; set; }
    }

    public class MusicInfoLoadEvent : IEventHandler<MusicInfoLoadEventData>, ITransientDependency
    {
        public IPluginManager PluginManager { get; set; }

        public async void HandleEvent(MusicInfoLoadEventData eventData)
        {
            List<MusicInfoModel> _infos = PluginManager.GetPlugin<IPluginAcquireMusicInfo>().GetMusicInfos(eventData.MusicFilePaths);

            await Task.Run(() =>
            {
                foreach (var _item in _infos)
                {

                }
            });
        }
    }
}
