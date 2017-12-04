using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZonyLrcTools.Common.Exceptions;

namespace ZonyLrcTools.Common
{
    public class FileSearchProvider : ISearchProvider
    {
        public Dictionary<string, List<string>> FindFiles(string directoryPath, IEnumerable<string> extensions)
        {
            if (extensions == null) throw new ArgumentNullException("搜索后缀不能为空!");
            if (extensions.Count() == 0) throw new ArgumentException("搜索后缀不能为空!");

            Dictionary<string, List<string>> _files = new Dictionary<string, List<string>>();
            foreach (var _ext in extensions)
            {
                try
                {
                    List<string> _result = new List<string>();
                    SearchFile(_result, directoryPath, _ext);
                    _files.Add(Path.GetExtension(_ext), _result);
                }
                catch (Exception)
                {
                    throw new FileSearchException("查找文件时失败，产生异常!");
                }
            }

            return _files;
        }

        public async Task<Dictionary<string, List<string>>> FindFilesAsync(string directoryPath, IEnumerable<string> extensions)
        {
            if (extensions == null) throw new ArgumentNullException("搜索后缀不能为空!");
            if (extensions.Count() == 0) throw new ArgumentException("搜索后缀不能为空!");

            Dictionary<string, List<string>> _files = new Dictionary<string, List<string>>();
            foreach (var _ext in extensions)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        List<string> _result = new List<string>();
                        SearchFile(_result, directoryPath, _ext);
                        _files.Add(Path.GetExtension(_ext), _result);
                    });
                }
                catch (Exception)
                {
                    throw new FileSearchException("查找文件时失败，产生异常!");
                }
            }

            return _files;
        }

        /// <summary>
        /// 递归文件搜索，忽略异常且无法访问的目录。
        /// </summary>
        /// <param name="files">输出的文件集合</param>
        /// <param name="folder">要搜索的目录</param>
        /// <param name="extension">要搜索的文件后缀名</param>
        private void SearchFile(List<string> files, string folder, string extension)
        {
            foreach (string file in Directory.GetFiles(folder, extension))
            {
                files.Add(file);
            }

            try
            {
                foreach (string directory in Directory.GetDirectories(folder))
                {
                    SearchFile(files, directory, extension);
                }
            }
            catch (Exception) { }
        }
    }
}