using System;
using System.Collections.Generic;

namespace ZonyLrcTools.Common
{
    [Serializable]
    public sealed class ConfigurationModel
    {
        /// <summary>
        /// 编码方式
        /// </summary>
        public string EncodingName { get; set; }
        /// <summary>
        /// 下载线程数量
        /// </summary>
        public int DownloadThreadNumber { get; set; }
        /// <summary>
        /// 是否忽略已存在歌词的文件
        /// </summary>
        public bool IsReplaceLyricFile { get; set; }
        /// <summary>
        /// 搜索的扩展名集合
        /// </summary>
        public List<string> ExtensionsName { get; set; }
        /// <summary>
        /// 是否检测更新
        /// </summary>
        public bool IsCheckUpdate { get; set; }
        /// <summary>
        /// 插件状态
        /// </summary>
        public List<PluginStatusModel> PluginStatuses;
        /// <summary>
        /// 是否同意用户协议
        /// </summary>
        public bool IsAgree { get; set; }
        /// <summary>
        /// 代理服务器地址
        /// </summary>
        public string ProxyIP { get; set; }
        /// <summary>
        /// 代理服务器端口
        /// </summary>
        public int ProxyPort { get; set; }

        /// <summary>
        /// 插件配置
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }
    }
}
