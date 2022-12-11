using MusicDecrypto.Library;

namespace ZonyLrcTools.Common.MusicDecryption
{
    /// <summary>
    /// 音乐解密器，用于将加密的歌曲数据，转换为可识别的歌曲格式。
    /// <remarks>
    /// 这个类型仅仅是对 <see cref="DecryptoFactory"/> 相关功能的封装，方便在本工具进行单元测试。
    /// </remarks>
    /// </summary>
    public interface IMusicDecryptor
    {
        /// <summary>
        /// 将加密数据歌曲文件转换为可识别的歌曲格式。
        /// </summary>
        /// <param name="filePath">加密歌曲文件的路径。</param>
        /// <returns>转换结果。</returns>
        Task<DecryptionResult> ConvertMusicAsync(string filePath);
    }
}