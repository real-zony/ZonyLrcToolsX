using Microsoft.Extensions.Options;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.IO;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Threading;
using ZonyLrcTools.Common.TagInfo;

namespace ZonyLrcTools.Common;

public class MusicInfoLoader : IMusicInfoLoader, ITransientDependency
{
    private readonly IWarpLogger _logger;
    private readonly IFileScanner _fileScanner;
    private readonly ITagLoader _tagLoader;
    private readonly GlobalOptions _options;

    public MusicInfoLoader(IWarpLogger logger,
        ITagLoader tagLoader,
        IFileScanner fileScanner,
        IOptions<GlobalOptions> options)
    {
        _logger = logger;
        _tagLoader = tagLoader;
        _fileScanner = fileScanner;
        _options = options.Value;
    }

    public async Task<List<MusicInfo?>> LoadAsync(string dirPath,
        int parallelCount = 2,
        CancellationToken cancellationToken = default)
    {
        var files = RemoveExistLyricFiles(await _fileScanner.ScanMusicFilesAsync(dirPath, _options.SupportFileExtensions));

        if (files.Count == 0)
        {
            await _logger.ErrorAsync("没有找到任何音乐文件。");
            throw new ErrorCodeException(ErrorCodes.NoFilesWereScanned);
        }

        await _logger.InfoAsync($"已经扫描到了 {files.Count} 个音乐文件。");

        return await LoadAsync(files, parallelCount, cancellationToken);
    }

    public async Task<List<MusicInfo?>> LoadAsync(IReadOnlyCollection<string> filePaths,
        int parallelCount = 2,
        CancellationToken cancellationToken = default)
    {
        await _logger.InfoAsync("开始加载音乐文件的标签信息...");

        var warpTask = new WarpTask(parallelCount);
        var warpTaskList = filePaths.Select(file =>
            warpTask.RunAsync(() =>
                    Task.Run(async () =>
                            await _tagLoader.LoadTagAsync(file),
                        cancellationToken),
                cancellationToken));

        var result = (await Task.WhenAll(warpTaskList))
            .Where(m => m != null)
            .Where(m => !string.IsNullOrEmpty(m?.Name) || !string.IsNullOrEmpty(m?.Artist))
            .ToList();

        await _logger.InfoAsync($"已成功加载 {filePaths.Count} 个音乐文件的标签信息。");

        return result;
    }

    private List<string> RemoveExistLyricFiles(IEnumerable<string> filePaths)
    {
        if (!_options.Provider.Lyric.Config.IsSkipExistLyricFiles)
        {
            return filePaths.ToList();
        }

        return filePaths
            .Where(path =>
            {
                if (!File.Exists(Path.ChangeExtension(path, ".lrc")))
                {
                    return true;
                }

                _logger.WarnAsync($"已经存在歌词文件 {path}，跳过。").GetAwaiter().GetResult();
                return false;
            })
            .ToList();
    }
}