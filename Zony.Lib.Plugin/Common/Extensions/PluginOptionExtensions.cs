using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Zony.Lib.Plugin.Common.Extensions
{
    public static class PluginOptionExtensions
    {
        /// <summary>
        /// 从插件配置拿取配置属性
        /// </summary>
        /// <typeparam name="TReturn">拿去的配置属性类型</typeparam>
        /// <param name="pluginAssembly">插件程序集，通过 typeof().Assembly 取得</param>
        /// <param name="key">属性 Key</param>
        /// <returns></returns>
        public static TReturn GetOptionValue<TReturn>(this Dictionary<string, Dictionary<string, object>> options, Assembly pluginAssembly, string key)
        {
            if (options == null) return default(TReturn);
            if (pluginAssembly == null) return default(TReturn);

            string pluginNameKey = Path.GetFileNameWithoutExtension(pluginAssembly.ManifestModule.Name);
            if (!options.ContainsKey(pluginNameKey)) return default(TReturn);
            if (!options[pluginNameKey].ContainsKey(key)) return default(TReturn);
            return (TReturn)options[pluginNameKey][key];
        }
    }
}