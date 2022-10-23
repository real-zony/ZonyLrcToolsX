using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Extensions;
using ZonyLrcTools.Common.Infrastructure.Logging;

namespace ZonyLrcTools.Common.Infrastructure.IO
{
    public class FileScanner : IFileScanner, ITransientDependency
    {
        private readonly IWarpLogger _logger;

        public FileScanner(IWarpLogger logger)
        {
            _logger = logger;
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
                _logger.LogWarningWithErrorCode(ErrorCodes.ScanFileError, e);
            }
        }
    }
}