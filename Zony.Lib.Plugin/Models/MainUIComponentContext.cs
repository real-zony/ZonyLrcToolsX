using System.Collections.Generic;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.UIComponents;

namespace Zony.Lib.Plugin.Models
{
    public class MainUIComponentContext : EventData
    {
        /// <summary>
        /// 中心音乐列表控件
        /// </summary>
        public ListViewNF Center_ListViewNF_MusicList { get; set; }
        /// <summary>
        /// 右侧专辑图像控件
        /// </summary>
        public PictureBox Right_PictureBox_AlbumImage { get; set; }
        /// <summary>
        /// 右侧音乐标题控件
        /// </summary>
        public TextBox Right_TextBox_MusicTitle { get; set; }
        /// <summary>
        /// 右侧音乐作者控件
        /// </summary>
        public TextBox Right_TextBox_MusicArtist { get; set; }
        /// <summary>
        /// 右侧音乐内置歌词控件
        /// </summary>
        public TextBox Right_TextBox_MusicBuildInLyric { get; set; }
        /// <summary>
        /// 中心音乐列表右键绑定的上下文菜单控件
        /// </summary>
        public ContextMenuStrip Center_ListVieNF_ContextMenuStrip { get; set; }
        /// <summary>
        /// 底部状态栏控件
        /// </summary>
        public StatusStrip Bottom_StatusStrip { get; set; }
        /// <summary>
        /// 顶部菜单控件
        /// </summary>
        public ToolStrip Top_ToolStrip { get; set; }
        /// <summary>
        /// 顶部按钮集合
        /// </summary>
        public Dictionary<string, ToolStripButton> Top_ToolStrip_Buttons { get; set; }
    }
}
