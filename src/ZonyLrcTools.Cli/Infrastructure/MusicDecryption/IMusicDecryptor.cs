using System.Threading.Tasks;

namespace ZonyLrcTools.Cli.Infrastructure.MusicDecryption
{
    /// <summary>
    /// 音乐解密器，用于将加密的歌曲数据，转换为可识别的歌曲格式。
    /// </summary>
    public interface IMusicDecryptor
    {
        /// <summary>
        /// 将加密数据转换为可识别的歌曲格式。
        /// </summary>
        /// <param name="sourceBytes">源加密的歌曲数据。</param>
        /// <returns>解密完成的歌曲数据。</returns>
        Task<byte[]> Convert(byte[] sourceBytes);
    }
}