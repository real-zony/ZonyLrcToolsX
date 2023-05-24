using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Common.Lyrics
{
    /// <summary>
    /// <see cref="ILyricsItemCollectionFactory"/> 的默认实现。
    /// </summary>
    public class LyricsItemCollectionFactory : ILyricsItemCollectionFactory, ITransientDependency
    {
        private readonly GlobalOptions _options;

        public LyricsItemCollectionFactory(IOptions<GlobalOptions> options)
        {
            _options = options.Value;
        }

        public LyricsItemCollection Build(string? sourceLyric)
        {
            var lyric = new LyricsItemCollection(_options.Provider.Lyric.Config);
            if (string.IsNullOrEmpty(sourceLyric))
            {
                return lyric;
            }

            InternalBuildLyricObject(lyric, sourceLyric);

            return lyric;
        }

        public LyricsItemCollection Build(string? sourceLyric, string? translationLyric)
        {
            var lyric = new LyricsItemCollection(_options.Provider.Lyric.Config);
            if (string.IsNullOrEmpty(sourceLyric))
            {
                return lyric;
            }

            lyric = InternalBuildLyricObject(lyric, sourceLyric);

            if (_options.Provider.Lyric.Config.IsEnableTranslation && !string.IsNullOrEmpty(translationLyric))
            {
                var translatedLyric = InternalBuildLyricObject(new LyricsItemCollection(_options.Provider.Lyric.Config), translationLyric);
                if (_options.Provider.Lyric.Config.IsOnlyOutputTranslation)
                {
                    return translatedLyric;
                }

                return lyric + translatedLyric;
            }

            return lyric;
        }

        private LyricsItemCollection InternalBuildLyricObject(LyricsItemCollection lyrics, string sourceText)
        {
            var regex = new Regex(@"\[\d+:\d+.\d+\].+\n?");
            foreach (Match match in regex.Matches(sourceText))
            {
                lyrics.Add(new LyricsItem(match.Value));
            }

            return lyrics;
        }
    }
}