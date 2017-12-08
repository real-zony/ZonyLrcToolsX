using System;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Plugin;
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
        }

        private void BindUIClickEvent()
        {
            #region > 搜索文件事件 <
            button_SearchFile.Click += delegate
            {
                EventBus.Default.Trigger(new SearchFileEventData()
                {
                    MusicListView = listView_SongItems
                });
            };
            #endregion

            #region > 歌曲信息加载事件 <
            listView_SongItems.Click += delegate
            {
                EventBus.Default.Trigger(new SingleMusicInfoLoadEventData()
                {

                });
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
        }
    }
}