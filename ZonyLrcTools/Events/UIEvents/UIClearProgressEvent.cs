using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Common;

namespace ZonyLrcTools.Events.UIEvents
{
    public class UIClearProgressEventData :EventData
    {

    }

    public class UIClearProgressEvent : IEventHandler<UIClearProgressEventData>,ITransientDependency
    {
        public void HandleEvent(UIClearProgressEventData eventData)
        {
            GlobalContext.Instance.UIContext.Bottom_ProgressBar.Maximum = GlobalContext.Instance.MusicInfos.Count;
            GlobalContext.Instance.UIContext.Bottom_ProgressBar.Value = 0;
        }
    }
}
