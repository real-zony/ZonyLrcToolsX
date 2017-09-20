using System;

namespace ZonyLrcTools.Common
{
    [Serializable]
    public sealed class PluginStatusModel
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string PluginName { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
