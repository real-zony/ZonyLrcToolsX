using System;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using ZonyLrcTools.Events;

namespace ZonyLrcTools.Forms
{
    public partial class Form_Startup : Form, ITransientDependency
    {

        public Form_Startup()
        {
            InitializeComponent();
        }

        private void Form_Startup_Load(object sender, EventArgs e)
        {
            BindUIClickEvent();
            BindUIClickEvent();
        }

        private void BindUIClickEvent()
        {
            button_SearchFile.Click += delegate { EventBus.Default.Trigger<ISearchFileEventData>(); };
            button_DownloadLyric.Click += delegate { EventBus.Default.Trigger<EventData>(); };
        }

        private void BindButtonEvent()
        {
            button_Setting.Click += delegate { IocManager.Instance.Resolve<Form_Setting>().Show(); };
            button_PluginsManager.Click += delegate { IocManager.Instance.Resolve<Form_PluginManager>().Show(); };
            button_Donate.Click += delegate { IocManager.Instance.Resolve<Form_Donate>().Show(); };
        }
    }
}
