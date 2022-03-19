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
            var lyric = new LyricItemCollection(_options.Provider.Lyric.Config);
            if (string.IsNullOrEmpty(sourceLyric))
            {
                return lyric;
            }

            InternalBuildLyricObject(lyric, sourceLyric);

            return lyric;
        }

        public LyricItemCollection Build(string sourceLyric, string translationLyric)
        {
            var lyric = new LyricItemCollection(_options.Provider.Lyric.Config);
            if (string.IsNullOrEmpty(sourceLyric))
            {
                return lyric;
            }

            lyric = InternalBuildLyricObject(lyric, sourceLyric);

            if (_options.Provider.Lyric.Config.IsEnableTranslation && !string.IsNullOrEmpty(translationLyric))
            {
                var translatedLyric = InternalBuildLyricObject(new LyricItemCollection(_options.Provider.Lyric.Config), translationLyric);
                return lyric + translatedLyric;
            }

            return lyric;
        }

        private LyricItemCollection InternalBuildLyricObject(LyricItemCollection lyric, string sourceText)
        {
            var regex = new Regex(@"\[\d+:\d+.\d+\].+\n?");
            foreach (Match match in regex.Matches(sourceText))
            {
                lyric.Add(new LyricItem(match.Value));
            }

            return lyric;
        }
    }
}