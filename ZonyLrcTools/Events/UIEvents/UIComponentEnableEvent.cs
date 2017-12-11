using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events.UIEvents
{
    public class UIComponentEnableEventData : EventData
    {

    }

    public class UIComponentEnableEvent : IEventHandler<UIComponentEnableEventData>,ITransientDependency
    {
        public void HandleEvent(UIComponentEnableEventData eventData)
        {
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_SearchFile].Enabled = true;
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadLyric].Enabled = true;
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadAblumImage].Enabled = true;
        }
    }
}