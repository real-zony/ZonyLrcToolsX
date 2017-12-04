using System.Collections.Generic;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common
{
    public interface ISearchProvider : ITransientDependency
    {
        Dictionary<string, List<string>> FindFiles(string directoryPath, IEnumerable<string> extensions);
        Task<Dictionary<string, List<string>>> FindFilesAsync(string directoryPath, IEnumerable<string> extensions);
    }
}