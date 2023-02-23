namespace ZonyLrcTools.Common.Infrastructure.Exceptions
{
    /// <summary>
    /// 错误码。
    /// </summary>
    public static class ErrorCodes
    {
        #region > 错误信息 <

        /// <summary>
        /// 文本: 待搜索的后缀不能为空。
        /// </summary>
        public const int FileSuffixIsEmpty = 10001;

        /// <summary>
        /// 文本: 需要扫描的目录不存在，请确认路径是否正确。。
        /// </summary>
        public const int DirectoryNotExist = 10002;

        /// <summary>
        /// 文本: 不能获取文件的后缀信息。
        /// </summary>
        public const int UnableToGetTheFileExtension = 10003;

        /// <summary>
        /// 文本: 没有扫描到任何音乐文件。
        /// </summary>
        public const int NoFilesWereScanned = 10004;

        /// <summary>
        /// 文本: 指定的编码不受支持，请检查配置，所有受支持的编码名称。
        /// </summary>
        public const int NotSupportedFileEncoding = 10005;
        
        /// <summary>
        /// 文本: 无法从网易云音乐获取歌曲列表。
        /// </summary>
        public const int UnableGetSongListFromNetEaseCloudMusic = 10006;

        #endregion

        #region > 警告信息 <

        /// <summary>
        /// 文本: 扫描文件时出现了错误。
        /// </summary>
        public const int ScanFileError = 50001;

        /// <summary>
        /// 文本: 歌曲名称或歌手名称均为空，无法进行搜索。
        /// </summary>
        public const int SongNameAndArtistIsNull = 50002;

        /// <summary>
        /// 文本: 歌曲名称不能为空，无法进行搜索。
        /// </summary>
        public const int SongNameIsNull = 50003;

        /// <summary>
        /// 文本: 下载器没有搜索到对应的歌曲信息。
        /// </summary>
        public const int NoMatchingSong = 50004;

        /// <summary>
        /// 文本: 下载请求的返回值不合法，可能是服务端故障。
        /// </summary>
        public const int TheReturnValueIsIllegal = 50005;

        /// <summary>
        /// 文本: 标签信息读取器为空，无法解析音乐 Tag 信息。
        /// </summary>
        public const int LoadTagInfoProviderError = 50006;

        /// <summary>
        /// 文本: TagLib 标签读取器出现了预期之外的异常。
        /// </summary>
        public const int TagInfoProviderLoadInfoFailed = 50007;

        /// <summary>
        /// 文本: 服务接口限制，无法进行请求，请尝试使用代理服务器。
        /// </summary>
        public const int ServiceUnavailable = 50008;

        /// <summary>
        /// 文本: 对目标服务器执行 Http 请求失败。
        /// </summary>
        public const int HttpRequestFailed = 50009;

        /// <summary>
        /// 文本: Http 请求的结果反序列化为 Json 失败。
        /// </summary>
        public const int HttpResponseConvertJsonFailed = 50010;

        /// <summary>
        /// 文本: 目前仅支持 NCM 格式的歌曲转换操作。
        /// </summary>
        public const int OnlySupportNcmFormatFile = 50011;

        #endregion
    }
}