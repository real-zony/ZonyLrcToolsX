using System;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Events;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Startup : Form, ITransientDependency
    {
        public IPluginManager PluginManager { get; set; }

        private MainUIComponentContext m_uiContext;

        public Form_Startup()
        {
            InitializeComponent();
        }

        private void Form_Startup_Load(object sender, EventArgs e)
        {
            BindUIClickEvent();
            BindButtonEvent();
            ComponentInitialize();
        }

        private void BindUIClickEvent()
        {
            #region > 搜索文件事件 <
            button_SearchFile.Click += delegate
            {
                EventBus.Default.Trigger(FillEventDataRelateUIComponents(new SearchFileEventData()));
            };
            #endregion

            #region > 歌曲信息加载事件 <
            listView_SongItems.Click += delegate
            {
                EventBus.Default.Trigger(FillEventDataRelateUIComponents(new SingleMusicInfoLoadEventData()));
            };
            #endregion

            #region > 歌词下载事件
            button_DownloadLyric.Click += delegate { EventBus.Default.Trigger(new MusicDownLoadEventData()); };
            #endregion
        }

        private void BindButtonEvent()
        {
            button_Setting.Click += delegate { IocManager.Instance.Resolve<Form_Setting>().Show(); };
            button_PluginsManager.Click += delegate { IocManager.Instance.Resolve<Form_PluginManager>().Show(); };
            button_Donate.Click += delegate { IocManager.Instance.Resolve<Form_Donate>().Show(); };
        }

        private void ComponentInitialize()
        {
            CheckForIllegalCrossThreadCalls = false;

            PluginManager.LoadPlugins();

            m_uiContext = new MainUIComponentContext()
            {
                Center_ListViewNF_MusicList = listView_SongItems,
                Right_PictureBox_AlbumImage = pictureBox_AlbumImg,
                Right_TextBox_MusicTitle = textBox_MusicTitle,
                Right_TextBox_MusicArtist = textBox_MusicArtist,
                Right_TextBox_MusicBuildInLyric = textBox_BuildInLyric,
                Top_ToolStrip = toolStrip1,
                Bottom_StatusStrip = statusStrip1
            };
        }

        private TEventData FillEventDataRelateUIComponents<TEventData>(TEventData eventData) where TEventData : MainUIComponentContext, new()
        {

            eventData.Center_ListViewNF_MusicList = m_uiContext.Center_ListViewNF_MusicList;
            eventData.Right_PictureBox_AlbumImage = m_uiContext.Right_PictureBox_AlbumImage;
            eventData.Right_TextBox_MusicTitle = m_uiContext.Right_TextBox_MusicTitle;
            eventData.Right_TextBox_MusicArtist = m_uiContext.Right_TextBox_MusicArtist;
            eventData.Right_TextBox_MusicBuildInLyric = m_uiContext.Right_TextBox_MusicBuildInLyric;
            eventData.Top_ToolStrip = m_uiContext.Top_ToolStrip;
            eventData.Bottom_StatusStrip = m_uiContext.Bottom_StatusStrip;
            return eventData;
        }
    }
}