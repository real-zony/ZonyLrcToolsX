namespace ZonyLrcTools.Cli.Infrastructure.Network
{
    /// <summary>
    /// 工具网络相关的设定。
    /// </summary>
    public class NetworkOptions
    {
        /// <summary>
        /// 是否启用了网络代理功能。
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 代理服务器的 Ip。
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 代理服务器的 端口。
        /// </summary>
        public int Port { get; set; }
    }
}