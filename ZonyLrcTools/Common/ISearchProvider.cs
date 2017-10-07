using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZonyLrcTools.Common
{
    public interface ISearchProvider
    {
        List<string> FindFiles(string directoryPath);
        Task<List<string>> FindFilesAsync(string diretcoryPath);
    }
}
