using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Net;

namespace ZonyLrcTools.Events
{
    public class CheckUpdateEventData : EventData
    {

    }

    public class CheckUpdateEvent : IEventHandler<CheckUpdateEventData>, ITransientDependency
    {
        public async void HandleEvent(CheckUpdateEventData eventData)
        {
            var result = await new HttpMethodUtils().GetAsync<UpdateModel>(@"http://api.myzony.com/api/VersionCheck/CheckVersion");
            var newVersion = new Version(result.Version);
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            string BuildMessageText()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("发现新版本，是否更新?").Append("\r\n");
                builder.Append("更新信息:").Append("\r\n");
                builder.Append(result.UpdateDescription.Replace("|","\r\n"));
                return builder.ToString();
            }

            if (newVersion > currentVersion)
            {
                if (MessageBox.Show(caption: "更新提示", text: BuildMessageText(), icon: MessageBoxIcon.Information, buttons: MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    Process.Start($"explorer {result.Url}");
                }
            }
        }

        private class UpdateModel
        {
            public string Version { get; set; }
            public string UpdateDescription { get; set; }
            public string Url { get; set; }
            public DateTime? UpdateTime { get; set; }
        }
    }
}
