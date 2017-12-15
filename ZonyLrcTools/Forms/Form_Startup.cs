using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;
using ZonyLrcTools.Events;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Startup : Form, ITransientDependency
    {
        public IPluginManager PluginManager { get; set; }

        public Form_Startup()
        {
            InitializeComponent();
        }

        private void Form_Startup_Load(object sender, EventArgs e)
        {
            BindUIClickEvent();
            BindButtonEvent();
            ComponentInitialize();
            InitializeParameters();
        }

        #region > 私有方法 < 

        /// <summary>
        /// 绑定 UI 事件
        /// </summary>
        private void BindUIClickEvent()
        {
            // 搜索文件事件
            button_SearchFile.Click += GenerateClickDelegate<SearchFileEventData>();
            // 单击歌曲加载信息事件
            listView_SongItems.Click += GenerateClickDelegate<SingleMusicInfoLoadEventData>();
            // 歌词下载事件
            button_DownloadLyric.Click += delegate { EventBus.Default.Trigger(new LyricDownLoadEventData(GlobalContext.Instance.GetConcurrentList())); };
            // 专辑图像下载事件
            button_DownloadAlbumImage.Click += delegate { EventBus.Default.Trigger(new AlbumdownloadEventData(GlobalContext.Instance.GetConcurrentList())); };
        }

        /// <summary>
        /// 绑定简单的窗口弹出事件
        /// </summary>
        private void BindButtonEvent()
        {
            button_Setting.Click += delegate { IocManager.Instance.Resolve<Form_Setting>().Show(); };
            button_PluginsManager.Click += delegate { IocManager.Instance.Resolve<Form_PluginManager>().Show(); };
            button_Donate.Click += delegate { IocManager.Instance.Resolve<Form_Donate>().Show(); };
        }

        /// <summary>
        /// 组件初始化
        /// </summary>
        private void ComponentInitialize()
        {
            CheckForIllegalCrossThreadCalls = false;

            PluginManager.LoadPlugins();

            GlobalContext.Instance.UIContext = new MainUIComponentContext()
            {
                Center_ListViewNF_MusicList = listView_SongItems,
                Right_PictureBox_AlbumImage = pictureBox_AlbumImg,
                Right_TextBox_MusicTitle = textBox_MusicTitle,
                Right_TextBox_MusicArtist = textBox_MusicArtist,
                Right_TextBox_MusicAblum = textBox_Ablum,
                Right_TextBox_MusicBuildInLyric = textBox_BuildInLyric,
                Top_ToolStrip = toolStrip1,
                Bottom_StatusStrip = statusStrip1,
                Top_ToolStrip_Buttons = BuildToolStripButtons()
            };
        }

        /// <summary>
        /// 初始化软件相关参数
        /// </summary>
        private void InitializeParameters()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
        }

        /// <summary>
        /// 填充 EventData 的 UI 组件数据
        /// </summary>
        /// <typeparam name="TEventData">EventData 类型</typeparam>
        /// <param name="eventData">要传递的事件数据</param>
        /// <returns></returns>
        private Dictionary<string, ToolStripButton> BuildToolStripButtons()
        {
            Dictionary<string, ToolStripButton> _resultDict = new Dictionary<string, ToolStripButton>();

            _resultDict.Add(AppConsts.Identity_Button_SearchFile, button_SearchFile);
            _resultDict.Add(AppConsts.Identity_Button_StopDownLoad, button_StopDownload);
            _resultDict.Add(AppConsts.Identity_Button_PluginManager, button_PluginsManager);
            _resultDict.Add(AppConsts.Identity_Button_DownLoadLyric, button_DownloadLyric);
            _resultDict.Add(AppConsts.Identity_Button_DownLoadAblumImage, button_DownloadLyric);
            _resultDict.Add(AppConsts.Identity_Button_Donate, button_Donate);
            _resultDict.Add(AppConsts.Identity_Button_Configuration, button_Setting);
            _resultDict.Add(AppConsts.Identity_Button_About, button_About);

            return _resultDict;
        }

        /// <summary>
        /// 生成简单的事件委托
        /// </summary>
        /// <typeparam name="TEventData">EventData 类型</typeparam>
        /// <param name="eventData">要传递的事件数据</param>
        /// <returns></returns>
        private EventHandler GenerateClickDelegate<TEventData>(TEventData eventData = null) where TEventData : EventData, new()
        {
            return delegate
            {
                if (eventData == null) EventBus.Default.Trigger(new TEventData());
            };
        }
        #endregion
    }
}