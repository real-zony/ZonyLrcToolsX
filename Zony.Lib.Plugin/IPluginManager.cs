using System.Collections.Generic;
using Zony.Lib.Infrastructures.Dependency;

namespace Zony.Lib.Plugin
{
    public interface IPluginManager : ITransientDependency
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
        void LoadPlugins();
        void LoadPlugins(string dirPath);
    }
}