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
        private readonly ConcurrentDictionary<Type, List<Type>> m_pluginContainer;
        private readonly string m_pluginsFolderPath = Environment.CurrentDirectory + @"\Plugins";

        public PluginManager()
        {
            m_pluginContainer = new ConcurrentDictionary<Type, List<Type>>();
        }

        public List<PluginInfoAttribute> GetAllPluginInfos()
        {
            var _result = new List<PluginInfoAttribute>();

            foreach (var _type in m_pluginContainer)
            {
                foreach (var _plugin in _type.Value)
                {
                    var _info = _plugin.GetCustomAttribute<PluginInfoAttribute>();
                    if (_info != null) _result.Add(_info);
                }
            }

            return _result;
        }

        public TInterface GetPlugin<TInterface>(Dictionary<string, Dictionary<string, object>> @params = null) where TInterface : class
        {
            Type _type = typeof(TInterface);

            if (m_pluginContainer.TryGetValue(_type, out List<Type> _plugins))
            {
                var _instance = Activator.CreateInstance(_plugins[0]) as IPlugin;
                _instance.PluginOptions = @params;
                return _instance as TInterface;
            }

            return default(TInterface);
        }

        public List<TInterface> GetPlugins<TInterface>(Dictionary<string, Dictionary<string, object>> @params = null) where TInterface : class
        {
            Type _type = typeof(TInterface);

            List<TInterface> _instances = new List<TInterface>();

            if (!m_pluginContainer.TryGetValue(_type, out List<Type> _plugins)) return null;
            _plugins.ForEach(_plugin =>
            {
                var _instance = Activator.CreateInstance(_plugin) as IPlugin;
                _instance.PluginOptions = @params;
                _instances.Add(_instance as TInterface);
            });
            return _instances;
        }

        public void LoadPlugins()
        {
            LoadPlugins(m_pluginsFolderPath);
        }

        public void LoadPlugins(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;

            string[] _files = Directory.GetFiles(dirPath, "*.dll");
            foreach (var _file in _files)
            {
                Assembly _asm = Assembly.UnsafeLoadFrom(_file);

                Type[] _types = _asm.GetTypes();
                List<Type> _plugins = new List<Type>();
                Type _interfaceType = null;

                foreach (var _type in _types)
                {
                    if (_type.GetInterface(typeof(IPlugin).Name) != null)
                    {
                        _interfaceType = _type.GetInterfaces().Where(x => x != typeof(IPlugin)).FirstOrDefault();

                        if (m_pluginContainer.ContainsKey(_interfaceType))
                        {
                            m_pluginContainer[_interfaceType].Add(_type);
                        }
                        else
                        {
                            _plugins.Add(_type);
                            m_pluginContainer.TryAdd(_interfaceType, _plugins);
                        }
                    }
                }
            }
        }
    }
}