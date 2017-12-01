using System.Collections.Generic;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common
{
    public interface ISearchProvider : ITransientDependency
    {
        List<string> FindFiles(string directoryPath);
        Task<List<string>> FindFilesAsync(string diretcoryPath);
    }
}