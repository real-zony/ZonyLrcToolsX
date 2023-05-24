namespace ZonyLrcTools.Common.Configuration
{
    public class TagInfoProviderOptions
    {
        public string Name { get; set; } = null!;

        public int Priority { get; set; }

        public Dictionary<string, string> Extensions { get; set; } = null!;
    }
}