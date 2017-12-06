using System.Collections.Generic;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;

namespace ZonyLrcTools.Common.Interfaces
{
    public interface IFileSearchProvider : ITransientDependency
    {
        /// <summary>
        /// 从指定文件夹当中搜索所有指定后缀的文件
        /// </summary>
        /// <param name="directoryPath">要搜索的目录</param>
        /// <param name="extensions">要搜索的后缀名集合</param>
        /// <returns></returns>
        Dictionary<string, List<string>> FindFiles(string directoryPath, IEnumerable<string> extensions);
        /// <summary>
        /// 从指定文件夹当中搜索所有指定后缀的文件
        /// </summary>
        /// <param name="directoryPath">要搜索的目录</param>
        /// <param name="extensions">要搜索的后缀名集合</param>
        /// <returns></returns>
        Task<Dictionary<string, List<string>>> FindFilesAsync(string directoryPath, IEnumerable<string> extensions);
    }
}