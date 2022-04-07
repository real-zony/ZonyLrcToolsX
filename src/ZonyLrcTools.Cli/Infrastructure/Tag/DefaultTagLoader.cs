using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    /// <summary>
    /// 默认的标签加载器 <see cref="ITagLoader"/> 实现。
    /// </summary>
    public class DefaultTagLoader : ITagLoader, ISingletonDependency
    {
        protected readonly IEnumerable<ITagInfoProvider> TagInfoProviders;
        protected readonly IBlockWordDictionary BlockWordDictionary;
        protected readonly ILogger<DefaultTagLoader> Logger;

        protected ToolOptions Options;
        
        private readonly IEnumerable<ITagInfoProvider> _sortedTagInfoProviders;

        public DefaultTagLoader(IEnumerable<ITagInfoProvider> tagInfoProviders,
            IBlockWordDictionary blockWordDictionary,
            IOptions<ToolOptions> options,
            ILogger<DefaultTagLoader> logger)
        {
            TagInfoProviders = tagInfoProviders;
            BlockWordDictionary = blockWordDictionary;
            Logger = logger;
            Options = options.Value;

            _sortedTagInfoProviders = GetTagInfoProviders();
        }

        public virtual async ValueTask<MusicInfo> LoadTagAsync(string filePath)
        {
            foreach (var provider in _sortedTagInfoProviders)
            {
                var info = await provider.LoadAsync(filePath);
                if (info != null)
                {
                    HandleBlockWord(info);
                    return info;
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
                .Join(TagInfoProviders,x=>x.Name,y=>y.Name,(x,y)=>y);
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