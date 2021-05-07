using System.Threading.Tasks;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    public interface ITagLoader
    {
        /// <summary>
        /// 加载歌曲的标签信息。
        /// </summary>
        /// <param name="filePath">歌曲文件的路径。</param>
        /// <returns>加载完成的歌曲信息。</returns>
        ValueTask<MusicInfo> LoadTagAsync(string filePath);
    }
}