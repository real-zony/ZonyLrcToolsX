using System;

namespace Zony.Lib.Plugin.Exceptions
{
    /// <summary>
    /// 无法找到歌词数据异常，该歌曲可能为纯音乐
    /// </summary>
    [Serializable]
    public class NotFoundLyricException : ApplicationException
    {
        public NotFoundLyricException() { }
        public NotFoundLyricException(string message) : base(message) { }
        public NotFoundLyricException(string message, Exception inner) : base(message, inner) { }
    }
}
