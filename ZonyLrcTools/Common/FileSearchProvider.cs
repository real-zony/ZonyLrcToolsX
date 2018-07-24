using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZonyLrcTools.Common.Exceptions;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Common
{
    public class FileSearchProvider : IFileSearchProvider
    {
        /// <summary>
        /// 从指定文件夹当中搜索所有指定后缀的文件
        /// </summary>
        /// <param name="directoryPath">要搜索的目录</param>
        /// <param name="extensions">要搜索的后缀名集合</param>
        /// <returns></returns>
        public Dictionary<string, List<string>> FindFiles(string directoryPath, IEnumerable<string> extensions)
        {
            if (extensions == null) throw new ArgumentNullException("搜索后缀不能为空!");
            if (!extensions.Any()) throw new ArgumentException("搜索后缀不能为空!");

            Dictionary<string, List<string>> files = new Dictionary<string, List<string>>();
            foreach (var ext in extensions)
            {
                try
                {
                    List<string> result = new List<string>();
                    SearchFile(result, directoryPath, ext);
                    files.Add(Path.GetExtension(ext) ?? throw new InvalidDataException("无法获得文件的后缀！"), result);
                }
                catch (Exception)
                {
                    throw new FileSearchException("查找文件时失败，产生异常!");
                }
            }

            return files;
        }

        /// <summary>
        /// 从指定文件夹当中搜索所有指定后缀的文件
        /// </summary>
        /// <param name="directoryPath">要搜索的目录</param>
        /// <param name="extensions">要搜索的后缀名集合</param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<string>>> FindFilesAsync(string directoryPath, IEnumerable<string> extensions)
        {
            if (extensions == null) throw new ArgumentNullException("搜索后缀不能为空!");
            if (!extensions.Any()) throw new ArgumentException("搜索后缀不能为空!");

            Dictionary<string, List<string>> files = new Dictionary<string, List<string>>();
            foreach (var ext in extensions)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        List<string> result = new List<string>();
                        SearchFile(result, directoryPath, ext);
                        files.Add(Path.GetExtension(ext) ?? throw new InvalidDataException("无法获得文件的后缀！"), result);
                    });
                }
                catch (Exception)
                {
                    throw new FileSearchException("查找文件时失败，产生异常!");
                }
            }

            return files;
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
            catch (Exception)
            {
                //
            }
        }
    }
}