namespace ZonyLrcTools.Common.Configuration
{
    /// <summary>
    /// 屏蔽词选项类。
    /// </summary>
    public class BlockWordOptions
    {
        /// <summary>
        /// 是否启用本功能。
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 屏蔽词字典文件，用于替换歌曲名或者歌手名称。
        /// </summary>
        public string FilePath { get; set; } = null!;
    }
}