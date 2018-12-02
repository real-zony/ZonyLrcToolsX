using System.Collections.Generic;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.NCMConverter
{
    [PluginInfo("NCM 加密文件转换插件", "Zony", "0.0.0.1", "http://blog.myzony.com", "转换 NCM 文件为 MP3/FLAC 文件。")]
    public class Startup : IPluginExtensions, IPlugin
    {
        public void InitializePlugin(IPluginManager plugManager)
        {
            GlobalContext.Instance.UIContext.AddPluginButton("NCM 转换插件", (sender, args) =>
            {
                var ui = new UI.NCMConverter
                {
                    Params = PluginOptions
                };

                ui.ShowDialog();
            });
        }

        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }
    }
}
