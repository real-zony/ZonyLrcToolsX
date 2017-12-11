using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events.UIEvents
{
    public class UIComponentDisableEventData : EventData
    {

    }

    public class UIComponentDisableEvent : IEventHandler<UIComponentDisableEventData>, ITransientDependency
    {
        public void HandleEvent(UIComponentDisableEventData eventData)
        {
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_SearchFile].Enabled = false;
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadLyric].Enabled = false;
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadAblumImage].Enabled = false;
        }
    }
}