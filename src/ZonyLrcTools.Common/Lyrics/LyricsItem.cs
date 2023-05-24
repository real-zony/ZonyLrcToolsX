using System.Text.RegularExpressions;

namespace ZonyLrcTools.Common.Lyrics
{
    /// <summary>
    /// 歌词的行对象，是 <see cref="LyricsItemCollection"/> 的最小单位。。
    /// </summary>
    public class LyricsItem : IComparable<LyricsItem>
    {
        /// <summary>
        /// 原始时间轴，格式类似于 [01:55.12]。
        /// </summary>
        public string OriginalTimeline => $"[{Minute:00}:{Second:00.00}]";

        /// <summary>
        /// 歌词文本数据。
        /// </summary>
        public string? LyricText { get; }

        /// <summary>
        /// 歌词所在的时间(分)。
        /// </summary>
        public int Minute { get; }

        /// <summary>
        /// 歌词所在的时间(秒)。
        /// </summary>
        public double Second { get; }

        /// <summary>
        /// 排序分数，用于一组歌词当中的排序权重。<br/>
        /// </summary>
        public double SortScore => Minute * 60 + Second;

        /// <summary>
        /// 构建新的 <see cref="LyricsItem"/> 对象。
        /// </summary>
        /// <param name="lyricText">原始的 Lyric 歌词。</param>
        public LyricsItem(string lyricText)
        {
            var timeline = new Regex(@"\[\d+:\d+.\d+\]").Match(lyricText)
                .Value.Replace("]", string.Empty)
                .Replace("[", string.Empty)
                .Split(':');

            if (int.TryParse(timeline[0], out var minute)) Minute = minute;
            if (double.TryParse(timeline[1], out var second)) Second = second;

            LyricText = new Regex(@"(?<=\[\d+:\d+.\d+\]).+").Match(lyricText).Value;
        }

        /// <summary>
        /// 构造新的 <see cref="LyricsItem"/> 对象。
        /// </summary>
        /// <param name="minute">歌词所在的时间(分)。</param>
        /// <param name="second">歌词所在的时间(秒)。</param>
        /// <param name="lyricText">歌词文本数据。</param>
        public LyricsItem(int minute, double second, string? lyricText)
        {
            Minute = minute;
            Second = second;
            LyricText = lyricText;
        }

        public int CompareTo(LyricsItem? other)
        {
            if (SortScore > other?.SortScore)
            {
                return 1;
            }

            if (SortScore < other?.SortScore)
            {
                return -1;
            }

            return 0;
        }

        public static bool operator >(LyricsItem left, LyricsItem right)
        {
            return left.SortScore > right.SortScore;
        }

        public static bool operator <(LyricsItem left, LyricsItem right)
        {
            return left.SortScore < right.SortScore;
        }

        public static bool operator ==(LyricsItem? left, LyricsItem? right)
        {
            return (int?)left?.SortScore == (int?)right?.SortScore;
        }

        public static bool operator !=(LyricsItem? item1, LyricsItem? item2)
        {
            return !(item1 == item2);
        }

        public static LyricsItem operator +(LyricsItem src, LyricsItem dist)
        {
            return new LyricsItem(src.Minute, src.Second, $"{src.LyricText}  {dist.LyricText}");
        }

        protected bool Equals(LyricsItem other)
        {
            return LyricText == other.LyricText && Minute == other.Minute && Second.Equals(other.Second);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LyricsItem)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LyricText, Minute, Second);
        }

        /// <summary>
        /// 获得歌词行的标准文本。
        /// </summary>
        public override string ToString() => $"[{Minute:00}:{Second:00.00}]{LyricText}";
    }
}