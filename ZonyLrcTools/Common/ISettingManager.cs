namespace ZonyLrcTools.Common
{
    /// <summary>
    /// 设置管理
    /// </summary>
    public interface ISettingManager
    {
        /// <summary>
        /// 加载设置
        /// </summary>
        void LoadConfiguration();
        /// <summary>
        /// 加载设置
        /// </summary>
        /// <param name="filePath">设置文件路径</param>
        void LoadConfiguration(string filePath);
        /// <summary>
        /// 保存设置
        /// </summary>
        void SaveConfiguration();
        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="filePath">设置文件路径</param>
        void SaveConfiguration(string filePath);
    }
}
