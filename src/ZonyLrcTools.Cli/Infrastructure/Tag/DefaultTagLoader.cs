using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public DefaultTagLoader(IEnumerable<ITagInfoProvider> tagInfoProviders)
        {
            TagInfoProviders = tagInfoProviders;
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
                    return info;
                }
            }

            return null;
        }
    }
}