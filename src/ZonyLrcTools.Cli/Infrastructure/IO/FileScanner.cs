using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;
using ZonyLrcTools.Cli.Infrastructure.Extensions;

namespace ZonyLrcTools.Cli.Infrastructure.IO
{
    public class FileScanner : IFileScanner, ITransientDependency
    {
        public ILogger<FileScanner> Logger { get; set; }

        public FileScanner()
        {
            Logger = NullLogger<FileScanner>.Instance;
        }

        public Task<List<FileScannerResult>> ScanAsync(string path, IEnumerable<string> extensions)
        {
            if (extensions == null || !extensions.Any())
            {
                throw new ErrorCodeException(ErrorCodes.FileSuffixIsEmpty);
            }

            if (!Directory.Exists(path))
            {
                throw new ErrorCodeException(ErrorCodes.DirectoryNotExist);
            }

            var files = new List<FileScannerResult>();
            foreach (var extension in extensions)
            {
                var tempResult = new ConcurrentBag<string>();
                SearchFile(tempResult, path, extension);

                files.Add(new FileScannerResult(
                    Path.GetExtension(extension) ?? throw new ErrorCodeException(ErrorCodes.UnableToGetTheFileExtension),
                    tempResult.ToList()));
            }

            return Task.FromResult(files);
        }

        private void SearchFile(ConcurrentBag<string> files, string folder, string extension)
        {
            try
            {
                foreach (var file in Directory.GetFiles(folder, extension))
                {
                    files.Add(file);
                }

                foreach (var directory in Directory.GetDirectories(folder))
                {
                    SearchFile(files, directory, extension);
                }
            }
            catch (Exception e)
            {
                Logger.LogWarningWithErrorCode(ErrorCodes.ScanFileError, e);
            }
        }
    }
}