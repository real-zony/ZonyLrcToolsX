using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    public class TagInfoProviderOption
    {
        public string Name { get; set; }

        public int Priority { get; set; }

        public Dictionary<string, string> Extensions { get; set; }
    }

    public class TagOption
    {
        public IEnumerable<TagInfoProviderOption> Plugin { get; set; }

        /// <summary>
        /// 屏蔽词功能相关配置。
        /// </summary>
        public BlockWordOption BlockWord { get; set; }
    }
}