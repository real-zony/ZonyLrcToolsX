using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZonyLrcTools.Cli.Infrastructure.Extensions;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    /// <summary>
    /// 歌词数据，包含多条歌词行对象(<see cref="LyricItem"/>)。
    /// </summary>
    public class LyricItemCollection : List<LyricItem>
    {
        /// <summary>
        /// 是否为纯音乐，当没有任何歌词数据的时候，属性值为 True。
        /// </summary>
        public bool IsPruneMusic => Count == 0;

        public LyricItemCollectionOption Option { get; private set; }

        public LyricItemCollection(LyricItemCollectionOption option)
        {
            Option = option;
        }

        public static LyricItemCollection operator +(LyricItemCollection left, LyricItemCollection right)
        {
            if (right.IsPruneMusic)
            {
                return left;
            }

            var option = left.Option;
            var newCollection = new LyricItemCollection(option);
            var indexDiff = left.Count - right.Count;

            if (!option.IsOneLine)
            {
                left.ForEach(item => newCollection.Add(item));
                right.ForEach(item => newCollection.Add(item));

                newCollection.Sort();
                return newCollection;
            }

            // 如果索引相等，直接根据索引快速匹配构建。
            if (indexDiff == 0)
            {
                newCollection.AddRange(left.Select((t, index) => t + right[index]));

                return newCollection;
            }

            // 首先按照时间轴进行合并。
            var leftMarkDict = BuildMarkDictionary(left);
            var rightMarkDict = BuildMarkDictionary(right);

            for (var leftIndex = 0; leftIndex < left.Count; leftIndex++)
            {
                var rightItem = right.Find(lyric => Math.Abs(lyric.SortScore - left[leftIndex].SortScore) < 0.001);
                if (rightItem != null)
                {
                    newCollection.Add(left[leftIndex] + rightItem);
                    var rightIndex = right.FindIndex(item => item == rightItem);
                    rightMarkDict[rightIndex] = true;
                }
                else
                {
                    newCollection.Add(left[leftIndex]);
                }

                leftMarkDict[leftIndex] = true;
            }

            // 遍历未处理的歌词项，将其添加到返回集合当中。
            var leftWaitProcessIndex = leftMarkDict
                .Where(item => item.Value)
                .Select(pair => pair.Key);
            var rightWaitProcessIndex = rightMarkDict
                .Where(item => item.Value)
                .Select(pair => pair.Key);

            leftWaitProcessIndex.Foreach(index => newCollection.Add(left[index]));
            rightWaitProcessIndex.Foreach(index => newCollection.Add(right[index]));

            newCollection.Sort();
            return newCollection;
        }

        /// <summary>
        /// 根据歌词集合构建一个索引状态字典。
        /// </summary>
        /// <remarks>
        /// 这个索引字典用于标识每个索引的歌词是否被处理，为 True 则为已处理，为 False 为未处理。
        /// </remarks>
        /// <param name="items">等待构建的歌词集合实例。</param>
        private static Dictionary<int, bool> BuildMarkDictionary(LyricItemCollection items)
        {
            return items
                .Select((item, index) => new {index, item})
                .ToDictionary(item => item.index, item => false);
        }

        public override string ToString()
        {
            var lyricBuilder = new StringBuilder();
            ForEach(lyric => lyricBuilder.Append(lyric).Append("\r\n"));
            return lyricBuilder.ToString().TrimEnd("\r\n");
        }
    }
}