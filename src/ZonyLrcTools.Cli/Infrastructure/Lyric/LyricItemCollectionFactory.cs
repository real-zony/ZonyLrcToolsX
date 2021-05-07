using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    public class LyricItemCollectionFactory : ILyricItemCollectionFactory, ITransientDependency
    {
        private readonly ToolOptions _options;

        public LyricItemCollectionFactory(IOptions<ToolOptions> options)
        {
            _options = options.Value;
        }

        public LyricItemCollection Build(string sourceLyric, string translateLyric = null)
        {
            var items = new LyricItemCollection(_options.LyricOption);
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