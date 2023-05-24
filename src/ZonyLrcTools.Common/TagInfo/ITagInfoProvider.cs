using System.Threading.Tasks;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    /// <summary>
    /// 具体的标签解析器，执行真正的标签解析逻辑。
    /// </summary>
    public interface ITagInfoProvider
    {
        /// <summary>
        /// 标签解析器的唯一标识。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 加载歌曲文件的标签信息。
        /// </summary>
        /// <param name="filePath">歌曲文件的路径。</param>
        /// <returns>加载完成的歌曲信息。</returns>
        ValueTask<MusicInfo?> LoadAsync(string filePath);
    }
}