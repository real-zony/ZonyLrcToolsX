using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.Plugin
{
    public class PluginManager : IPluginManager
    {
        private readonly ConcurrentDictionary<Type, List<Type>> _pluginContainer;
        private readonly string _pluginsFolderPath = Environment.CurrentDirectory + @"\Plugins";

        public PluginManager()
        {
            _pluginContainer = new ConcurrentDictionary<Type, List<Type>>();
        }

        /// <summary>
        /// 获得所有插件信息
        /// </summary>
        public List<PluginInfoAttribute> GetAllPluginInfos()
        {
            var result = new List<PluginInfoAttribute>();

            foreach (var type in _pluginContainer)
            {
                foreach (var plugin in type.Value)
                {
                    var info = plugin.GetCustomAttribute<PluginInfoAttribute>();
                    if (info != null) result.Add(info);
                }
            }

            return result;
        }

        /// <summary>
        /// 获得指定类型的插件
        /// </summary>
        /// <typeparam name="TInterface">插件类型</typeparam>
        /// <returns>获得到的插件单例对象</returns>
        public TInterface GetPlugin<TInterface>(Dictionary<string, Dictionary<string, object>> @params = null) where TInterface : class
        {
            Type type = typeof(TInterface);

            if (_pluginContainer.TryGetValue(type, out List<Type> plugins))
            {
                if (Activator.CreateInstance(plugins[0]) is IPlugin instance)
                {
                    instance.PluginOptions = @params;
                    return instance as TInterface;
                }
            }

            return default(TInterface);
        }

        /// <summary>
        /// 获得指定类型的插件
        /// </summary>
        /// <typeparam name="TInterface">插件类型</typeparam>
        /// <returns>插件实例列表</returns>
        public List<TInterface> GetPlugins<TInterface>(Dictionary<string, Dictionary<string, object>> @params = null) where TInterface : class
        {
            Type type = typeof(TInterface);

            List<TInterface> instances = new List<TInterface>();

            if (!_pluginContainer.TryGetValue(type, out List<Type> plugins)) return null;
            plugins.ForEach(plugin =>
            {
                if (Activator.CreateInstance(plugin) is IPlugin instance)
                {
                    instance.PluginOptions = @params;
                    instances.Add(instance as TInterface);
                }
            });
            return instances;
        }

        /// <summary>
        /// 从默认目录加载插件
        /// </summary>
        public void LoadPlugins()
        {
            LoadPlugins(_pluginsFolderPath);
        }

        /// <summary>
        /// 从指定的路径加载插件
        /// </summary>
        /// <param name="dirPath">插件路径</param>
        public void LoadPlugins(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;

            string[] files = Directory.GetFiles(dirPath, "*.dll");
            foreach (var file in files)
            {
                Assembly asm = Assembly.UnsafeLoadFrom(file);

                Type[] types = asm.GetTypes();
                List<Type> plugins = new List<Type>();

                // 遍历程序集所有类型
                foreach (var type in types)
                {
                    // 判断类型是否为插件(IPlugin 类型)
                    if (type.GetInterface(typeof(IPlugin).Name) != null)
                    {
                        // 查找具体的接口类型，例如 IPluginDownloader 类型，如果 Container 已经存在该类型的键的话，获取其
                        // 键关联的 List,将该类型加入，否则新建一个键值对
                        var interfaceType = type.GetInterfaces().FirstOrDefault(x => x != typeof(IPlugin));

                        if (_pluginContainer.ContainsKey(interfaceType ?? throw new InvalidOperationException("无效接口类型，请确定插件只实现了一个插件接口.")))
                        {
                            _pluginContainer[interfaceType].Add(type);
                        }
                        else
                        {
                            plugins.Add(type);
                            _pluginContainer.TryAdd(interfaceType, plugins);
                        }
                    }
                }
            }
        }
    }
}