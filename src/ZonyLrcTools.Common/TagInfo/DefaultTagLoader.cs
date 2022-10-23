using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Infrastructure.Tag;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;

namespace ZonyLrcTools.Common.TagInfo
{
    /// <summary>
    /// 默认的标签加载器 <see cref="ITagLoader"/> 实现。
    /// </summary>
    public class DefaultTagLoader : ITagLoader, ISingletonDependency
    {
        protected readonly IEnumerable<ITagInfoProvider> TagInfoProviders;
        protected readonly IBlockWordDictionary BlockWordDictionary;
        protected readonly ILogger<DefaultTagLoader> Logger;

        protected GlobalOptions Options;

        private readonly IEnumerable<ITagInfoProvider> _sortedTagInfoProviders;

        public DefaultTagLoader(IEnumerable<ITagInfoProvider> tagInfoProviders,
            IBlockWordDictionary blockWordDictionary,
            IOptions<GlobalOptions> options,
            ILogger<DefaultTagLoader> logger)
        {
            TagInfoProviders = tagInfoProviders;
            BlockWordDictionary = blockWordDictionary;
            Logger = logger;
            Options = options.Value;

            _sortedTagInfoProviders = GetTagInfoProviders();
        }

        public virtual async ValueTask<MusicInfo?> LoadTagAsync(string filePath)
        {
            foreach (var provider in _sortedTagInfoProviders)
            {
                try
                {
                    var info = await provider.LoadAsync(filePath);
                    if (info != null && !string.IsNullOrEmpty(info.Name))
                    {
                        HandleBlockWord(info);
                        return info;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Logger.LogWarning($"{filePath} 没有找到正确的标签信息，请考虑调整正则表达式。");
            return null;
        }

        private IEnumerable<ITagInfoProvider> GetTagInfoProviders()
        {
            if (!TagInfoProviders.Any())
            {
                throw new ErrorCodeException(ErrorCodes.LoadTagInfoProviderError);
            }

            return Options.Provider.Tag.Plugin
                .Where(x => x.Priority != -1)
                .OrderBy(x => x.Priority)
                .Join(TagInfoProviders, x => x.Name, y => y.Name, (x, y) => y);
        }

        protected void HandleBlockWord(MusicInfo info)
        {
            if (Options.Provider.Tag.BlockWord.IsEnable)
            {
                info.Name = BlockWordDictionary.GetValue(info.Name) ?? info.Name;
                info.Artist = BlockWordDictionary.GetValue(info.Name) ?? info.Artist;
            }
        }
    }
}