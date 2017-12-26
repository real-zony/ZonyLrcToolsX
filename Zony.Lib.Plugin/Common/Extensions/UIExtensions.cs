namespace Zony.Lib.Plugin.Common.Extensions
{
    public static class UIExtensions
    {
        /// <summary>
        /// 在音乐列表当中设置指定项的状态文本
        /// </summary>
        /// <param name="itemIndex">条目索引</param>
        /// <param name="statusText">状态文本,建议使用 AppConsts 常量文本</param>
        public static void SetItemStatus(this GlobalContext context, int itemIndex, string statusText)
        {
            if (GlobalContext.Instance.UIContext == null) return;
            if (GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList == null) return;

            // 这里 4 在 ZonyLrcTools 主项目当中可使用 "AppConsts.Status_Position" 来代替
            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[itemIndex].SubItems[4].Text = statusText;
        }
    }
}
