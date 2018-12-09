using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zony.Lib.Infrastructures.Lyrics
{
    public class LyricItemCollection : List<LyricItem>
    {
        public LyricItemCollection()
        {

        }

        public LyricItemCollection(string srcLyricText)
        {
            if (string.IsNullOrEmpty(srcLyricText)) throw new ArgumentNullException("源歌词文本数据为空，请传入有效文本。");

            Regex regex = new Regex(@"\[\d+:\d+.\d+\].+\n");
            var result = regex.Matches(srcLyricText);

            foreach (Match match in result)
            {
                Add(new LyricItem(match.Value));
            }
        }

        /// <summary>
        /// 生成歌词数据
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var lyricItem in this)
            {
                sb.Append(lyricItem).Append('\n');
            }

            return sb.ToString().TrimEnd('\n');
        }

        /// <summary>
        /// 尝试合并歌词条目
        /// </summary>
        /// <param name="otherCollection">待合并的歌词条目集合</param>
        /// <param name="intoOneLine">是否合并为一行</param>
        /// <remarks><see cref="intoOneLine"/> 参数只能保证时间轴一致的情况能够合并为一行，如果不一致
        /// 则可能仍会分为两行进行处理。合并处理依据以源歌词为基准进行，一旦出现源歌词与翻译歌词数目不匹配
        /// 的情况会造成分行处理，但最终的结果仍然以升序排列。</remarks>
        /// <returns>合并成功的歌词结果</returns>
        public LyricItemCollection Merge(LyricItemCollection otherCollection,bool intoOneLine = true)
        {
            var lyricItemCollection = new LyricItemCollection();
            var indexDiff = this.Count - otherCollection.Count;

            if (intoOneLine)
            {
                // 如果索引数相等，直接根据索引快速构建
                if (indexDiff == 0)
                {
                    for (int index = 0; index < this.Count; index++)
                    {
                        lyricItemCollection.Add(this[index] + otherCollection[index]);
                    }

                    return lyricItemCollection;
                }

                // 处理存在两种情况的歌词，一种是时间轴匹配进行合并，剩余的则按照未处理的顺序进行添加。
                var srcMarkDict = new Dictionary<int, bool>();
                var distMarkDict = new Dictionary<int, bool>();
                for (int srcIndex = 0; srcIndex < this.Count; srcIndex++)
                {
                    srcMarkDict.Add(srcIndex,false);
                }

                for (int distIndex = 0; distIndex < otherCollection.Count; distIndex++)
                {
                    distMarkDict.Add(distIndex,false);
                }

                // 优先处理匹配项
                for (int srcIndex = 0; srcIndex < this.Count; srcIndex++)
                {
                    var otherItem = otherCollection.Find(x => x.SortTimeLine == this[srcIndex].SortTimeLine);
                    if (otherItem != null)
                    {
                        lyricItemCollection.Add(this[srcIndex] + otherItem);
                        
                        var distIndex = otherCollection.FindIndex(x=>x == otherItem);
                        distMarkDict[distIndex] = true;
                    }
                    else
                    {
                        lyricItemCollection.Add(this[srcIndex]);
                    }

                    srcMarkDict[srcIndex] = true;
                }

                // 遍历处理未匹配条目
                var srcWaitProcessIndex = srcMarkDict.Where(item => item.Value == false).Select(pair=>pair.Key).ToList();
                var distWaitProcessIndex = distMarkDict.Where(item => item.Value == false).Select(pair=>pair.Key).ToList();

                srcWaitProcessIndex.ForEach(index=>lyricItemCollection.Add(this[index]));
                distWaitProcessIndex.ForEach(index=>lyricItemCollection.Add(otherCollection[index]));

                lyricItemCollection.Sort();
                return lyricItemCollection;
            }
            
            this.ForEach(item=>lyricItemCollection.Add(item));
            otherCollection.ForEach(item => lyricItemCollection.Add(item));

            lyricItemCollection.Sort();
            return lyricItemCollection;
        }
    }
}
