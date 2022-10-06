using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    /// <summary>
    /// <see cref="ILyricItemCollectionFactory"/> 的默认实现。
    /// </summary>
    public class LyricItemCollectionFactory : ILyricItemCollectionFactory, ITransientDependency
    {
        private readonly GlobalOptions _options;

        public LyricItemCollectionFactory(IOptions<GlobalOptions> options)
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
                if (_options.Provider.Lyric.Config.IsOnlyOutputTranslation)
                {
                    return translatedLyric;
                }

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