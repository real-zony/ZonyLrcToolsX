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
            GlobalContext.Instance.SetBottomStatusText(AppConsts.Status_Bottom_LoadingMusicinfo);
            List<MusicInfoModel> infos = (await PluginManager.GetPlugin<IPluginAcquireMusicInfo>().GetMusicInfosAsync(eventData.MusicFilePaths)).Where(z => z != null).OrderBy(z => z.Index).ToList();
            await Task.Run(() => GlobalContext.Instance.InsertMusicInfosAndFillListView(infos));
            GlobalContext.Instance.SetBottomStatusText(AppConsts.Status_Bottom_LoadMusicInfoComplete);
            EventBus.Default.Trigger<UIComponentEnableEventData>();
        }
    }
}