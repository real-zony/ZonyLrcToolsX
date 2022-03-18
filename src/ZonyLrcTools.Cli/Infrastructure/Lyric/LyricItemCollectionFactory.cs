using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    /// <summary>
    /// <see cref="ILyricItemCollectionFactory"/> 的默认实现。
    /// </summary>
    public class LyricItemCollectionFactory : ILyricItemCollectionFactory, ITransientDependency
    {
        private readonly ToolOptions _options;

        public LyricItemCollectionFactory(IOptions<ToolOptions> options)
        {
            _options = options.Value;
        }

        public LyricItemCollection Build(string sourceLyric)
        {
            var items = new LyricItemCollection(_options.Provider.Lyric.Config);
            if (string.IsNullOrEmpty(sourceLyric))
            {
                return items;
            }

            var regex = new Regex(@"\[\d+:\d+.\d+\].+\n?");
            foreach (Match match in regex.Matches(sourceLyric))
            {
                items.Add(new LyricItem(match.Value));
            }

            return items;
        }
    }
}