using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    /// <summary>
    /// 默认的标签加载器 <see cref="ITagLoader"/> 实现。
    /// </summary>
    public class DefaultTagLoader : ITagLoader, ITransientDependency
    {
        protected readonly IEnumerable<ITagInfoProvider> TagInfoProviders;
        protected readonly IBlockWordDictionary BlockWordDictionary;
        protected ToolOptions Options;

        public DefaultTagLoader(IEnumerable<ITagInfoProvider> tagInfoProviders,
            IBlockWordDictionary blockWordDictionary,
            IOptions<ToolOptions> options)
        {
            TagInfoProviders = tagInfoProviders;
            BlockWordDictionary = blockWordDictionary;
            Options = options.Value;
        }

        public virtual async ValueTask<MusicInfo> LoadTagAsync(string filePath)
        {
            if (!TagInfoProviders.Any())
            {
                throw new ErrorCodeException(ErrorCodes.LoadTagInfoProviderError);
            }

            foreach (var provider in TagInfoProviders.OrderBy(p => p.Priority))
            {
                var info = await provider.LoadAsync(filePath);
                if (info != null)
                {
                    HandleBlockWord(info);
                    return info;
                }
            }

            return null;
        }

        protected void HandleBlockWord(MusicInfo info)
        {
            if (Options.BlockWordOptions.IsEnable)
            {
                info.Name = BlockWordDictionary.GetValue(info.Name) ?? info.Name;
                info.Artist = BlockWordDictionary.GetValue(info.Name) ?? info.Artist;
            }
        }
    }
}