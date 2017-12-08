namespace Zony.Lib.Plugin.Models
{
    /// <summary>
    /// 歌曲信息模型
    /// </summary>
    public class MusicInfoModel
    {
        /// <summary>
        /// 歌手/艺术家
        /// </summary>
        public string Artist { get; set; }
        /// <summary>
        /// 歌曲名称
        /// </summary>
        public string Song { get; set; }
        /// <summary>
        /// 歌曲文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 专辑/唱片名称
        /// </summary>
        public string Album { get; set; }
        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extensions { get; set; }
        /// <summary>
        /// 歌曲标签类型
        /// </summary>
        public string TagType { get; set; }

        /// <summary>
        /// 是否下载成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 是否有专辑图像
        /// </summary>
        public bool IsAlbumImg { get; set; }
        /// <summary>
        /// 是否有内置歌词
        /// </summary>
        public bool IsBuildInLyric { get; set; }
        /// <summary>
        /// 内置歌词
        /// </summary>
        public string BuildInLyric { get; set; }
    }
}
