using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common.Interfaces
{
    /// <summary>
    /// 设置管理
    /// </summary>
    public interface IConfigurationManager : ISingletonDependency
    {
        /// <summary>
        /// 从默认配置文件加载设置
        /// </summary>
        void LoadConfiguration();

        /// <summary>
        /// 加载设置
        /// </summary>
        /// <param name="filePath">设置文件路径</param>
        void LoadConfiguration(string filePath);

        /// <summary>
        /// 保存设置到默认配置文件
        /// </summary>
        void SaveConfiguration();

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="filePath">设置文件路径</param>
        void SaveConfiguration(string filePath);

        /// <summary>
        /// 配置模型
        /// </summary>
        ConfigurationModel ConfigModel { get; }
    }
}