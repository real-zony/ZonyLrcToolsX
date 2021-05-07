using System.Threading.Tasks;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    public interface ITagInfoProvider
    {
        int Priority { get; }

        string Name { get; }

        ValueTask<MusicInfo> LoadAsync(string filePath);
    }
}