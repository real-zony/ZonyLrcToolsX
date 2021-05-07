using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    public class FileNameTagInfoProvider : ITagInfoProvider, ITransientDependency
    {
        public int Priority => 2;

        public string Name => ConstantName;
        public const string ConstantName = "FileName";

        private readonly ToolOptions Options;

        public FileNameTagInfoProvider(IOptions<ToolOptions> options)
        {
            Options = options.Value;
        }

        public async ValueTask<MusicInfo> LoadAsync(string filePath)
        {
            await ValueTask.CompletedTask;

            var match = Regex.Match(Path.GetFileNameWithoutExtension(filePath), Options.TagInfoProviderOptions.FileNameRegularExpressions);

            if (match.Groups.Count != 3)
            {
                return null;
            }

            return new MusicInfo(filePath, match.Groups["name"].Value, match.Groups["artist"].Value);
        }
    }
}