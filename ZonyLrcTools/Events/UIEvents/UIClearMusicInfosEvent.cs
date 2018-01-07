using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Common;

namespace ZonyLrcTools.Events.UIEvents
{
    public class UIClearMusicInfosEventData : EventData
    {

    }

    public class UIClearMusicInfosEvent : IEventHandler<UIClearMusicInfosEventData>, ITransientDependency
    {
        public void HandleEvent(UIClearMusicInfosEventData eventData)
        {
            GlobalContext.Instance.MusicInfos.Clear();
            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items.Clear();
        }
    }
}
