using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Events.UIEvents;

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
            List<MusicInfoModel> _infos = await Task.Run(() =>
            {
                return PluginManager.GetPlugin<IPluginAcquireMusicInfo>().GetMusicInfos(eventData.MusicFilePaths).OrderBy(z => z.Index).ToList();
            });

            GlobalContext.Instance.SetBottomStatusText(AppConsts.Status_Bottom_LoadingMusicinfo);
            await Task.Run(() => GlobalContext.Instance.InsertMusicInfosAndFillListView(_infos));
            GlobalContext.Instance.SetBottomStatusText(AppConsts.Status_Bottom_LoadMusicInfoComplete);

            EventBus.Default.Trigger<UIComponentEnableEventData>();
        }
    }
}