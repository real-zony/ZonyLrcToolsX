using Zony.Lib.Infrastructures.EventBus.Handlers;
using ZonyLrcTools.Common;
using Zony.Lib.Infrastructures.EventBus;

namespace ZonyLrcTools.Events.UIEvents
{
    public class UIComponentEnableEventData : EventData
    {

    }

    public class UIComponentEnableEvent : IEventHandler<UIComponentDisableEventData>
    {
        public void HandleEvent(UIComponentDisableEventData eventData)
        {
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_SearchFile].Enabled = true;
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadLyric].Enabled = true;
            GlobalContext.Instance.UIContext.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadAblumImage].Enabled = true;
        }
    }
}