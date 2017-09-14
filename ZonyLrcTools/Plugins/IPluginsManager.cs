namespace ZonyLrcTools.Plugins
{
    /// <summary>
    /// 插件管理器
    /// </summary>
    internal interface IPluginsManager
    {
        /// <summary>
        /// 加载指定目录下的所有插件，并返回加载成功的数量
        /// </summary>
        /// <returns></returns>
        int LoadPlugins();
        /// <summary>
        /// 加载指定插件
        /// </summary>
        /// <param name="filePath">插件路径</param>
        /// <returns>是否加载成功</returns>
        bool LoadPlugins(string filePath);
        /// <summary>
        /// 清空插件容器
        /// </summary>
        void ClearContainer();
    }
}
