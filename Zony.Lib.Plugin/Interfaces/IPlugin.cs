using System.Collections.Generic;

namespace Zony.Lib.Plugin.Interfaces
{
    /// <summary>
    /// 表示这个类属于一个插件
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 动态插件参数
        /// </summary>
        Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }
    }
}
