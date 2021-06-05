using System.Collections.Generic;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    public class TagInfoProviderInstance
    {
        public string Name { get; set; }

        public int Priority { get; set; }

        public Dictionary<string, string> Extensions { get; set; }
    }
}