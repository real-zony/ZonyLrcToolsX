using System.Collections.Generic;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Plugin.Attributes;

namespace Zony.Lib.Plugin
{
    public interface IPluginManager : ISingletonDependency
    {
        /// <summary>
        /// 获得指定类型的插件
        /// </summary>
        /// <typeparam name="TInterface">插件类型</typeparam>
        /// <returns>获得到的插件单例对象</returns>
        TInterface GetPlugin<TInterface>() where TInterface : class;
        /// <summary>
        /// 获得指定类型的插件
        /// </summary>
        /// <typeparam name="TInterface">插件类型</typeparam>
        /// <returns>插件实例列表</returns>
        List<TInterface> GetPlugins<TInterface>() where TInterface : class;
        /// <summary>
        /// 从默认目录加载插件
        /// </summary>
        void LoadPlugins();
        /// <summary>
        /// 从指定的路径加载插件
        /// </summary>
        /// <param name="dirPath">插件路径</param>
        void LoadPlugins(string dirPath);
        /// <summary>
        /// 获得所有插件信息
        /// </summary>
        List<PluginInfoAttribute> GetAllPluginInfos();
    }
}