using System;
using System.Linq;
using System.Windows.Forms;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Common.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class UIExtensions
    {
        /// <summary>
        /// 在音乐列表当中设置指定项的状态文本
        /// </summary>
        /// <param name="context">全局上下文</param>
        /// <param name="itemIndex">条目索引</param>
        /// <param name="statusText">状态文本,建议使用 AppConsts 常量文本</param>
        public static void SetItemStatus(this GlobalContext context, int itemIndex, string statusText)
        {
            if (GlobalContext.Instance.UIContext == null) return;
            if (GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList == null) return;

            // 这里 4 在 ZonyLrcTools 主项目当中可使用 "AppConsts.Status_Position" 来代替
            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[itemIndex].SubItems[4].Text = statusText;
        }

        /// <summary>
        /// 设置底部状态栏文本内容
        /// </summary>
        /// <param name="context">全局上下文</param>
        /// <param name="text">要设置的内容文本</param>
        public static void SetBottomStatusText(this GlobalContext context, string text)
        {
            if (GlobalContext.Instance.UIContext == null) return;
            if (GlobalContext.Instance.UIContext.Bottom_StatusStrip == null) return;

            GlobalContext.Instance.UIContext.Bottom_StatusStrip.Text = text;
        }

        /// <summary>
        /// 向顶部菜单的插件功能按钮添加子按钮
        /// </summary>
        /// <param name="context">UI 上下文</param>
        /// <param name="buttonText">按钮名称</param>
        /// <param name="buttonClickEvent">绑定的按钮操作委托</param>
        public static ToolStripItem AddPluginButton(this MainUIComponentContext context, string buttonText, EventHandler buttonClickEvent)
        {
            ToolStripDropDownButton subButton = context.Top_ToolStrip.Items.Cast<ToolStripItem>().FirstOrDefault(z => z.Name == AppConsts.Identity_PluginButtons) as ToolStripDropDownButton;
            if (subButton == null)
            {
                int index = context.Top_ToolStrip.Items.Add(new ToolStripDropDownButton("插件功能") { Name = AppConsts.Identity_PluginButtons });
                subButton = context.Top_ToolStrip.Items[index] as ToolStripDropDownButton;
            }

            if (subButton == null) throw new Exception("产生严重程序异常!");
            var button = subButton.DropDownItems.Add(buttonText);
            button.Click += buttonClickEvent;
            return button;
        }

        /// <summary>
        /// 启用按钮组
        /// </summary>
        public static void EnableTopButtons(this MainUIComponentContext context)
        {
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_SearchFile].Enabled = true;
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadLyric].Enabled = true;
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_StopDownLoad].Enabled = true;
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadAblumImage].Enabled = true;

            var plugButton = context.Top_ToolStrip.Items.Cast<ToolStripItem>().FirstOrDefault(z => z.Name == AppConsts.Identity_PluginButtons);
            if (plugButton != null) plugButton.Enabled = true;
        }

        /// <summary>
        /// 禁用按钮组
        /// </summary>
        public static void DisableTopButtons(this MainUIComponentContext context)
        {
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_SearchFile].Enabled = false;
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadLyric].Enabled = false;
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_StopDownLoad].Enabled = false;
            context.Top_ToolStrip_Buttons[AppConsts.Identity_Button_DownLoadAblumImage].Enabled = false;

            var plugButton = context.Top_ToolStrip.Items.Cast<ToolStripItem>().FirstOrDefault(z => z.Name == AppConsts.Identity_PluginButtons);
            if (plugButton != null) plugButton.Enabled = false;
        }
    }
}

