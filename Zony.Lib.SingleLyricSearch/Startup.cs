using Newtonsoft.Json;
using System.Linq;
using System.Windows.Forms;
using Zony.Lib.Net;
using Zony.Lib.Net.JsonModels.NetEase;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.SingleLyricSearch
{
    [PluginInfo("网易云歌单获取插件", "Zony", "1.0.0.0", "http://www.myzony.com", "从用户给定的歌单 URL 当中获取歌曲信息。")]
    public class Startup : IPluginExtensions, IPlugin
    {
        private readonly HttpMethodUtils m_clinet = new HttpMethodUtils();

        public void Action1()
        {
            var _jsonResult = m_clinet.Get("http://music.163.com/api/playlist/detail",
                new
                {
                    id = 367080297,
                    offset = 0,
                    total = true,
                    limit = 1000,
                    n = 1000,
                    csrf_token = string.Empty
                }, "http://music.163.com");
            NetEaseResultModel _jsonModel = JsonConvert.DeserializeObject<NetEaseResultModel>(_jsonResult);

        }

        public void InitializePlugin(IPluginManager plugManager)
        {
            GlobalContext.Instance.UIContext.Top_ToolStrip.Items.Add("AS");
        }

        private void InitializeDropdownButton()
        {
            GlobalContext.Instance.UIContext.Top_ToolStrip.Items.Add(new ToolStripDropDownButton("x1") { Name = "Identity_PluginButtons" });

            if (GlobalContext.Instance.UIContext.Top_ToolStrip.Items.Cast<ToolStripItem>().FirstOrDefault(z => z.Name == "Identity_PluginButtons") != null)
            {
            }
            
            //GlobalContext.Instance.UIContext.Top_ToolStrip.Items.Add();

        }
    }
}