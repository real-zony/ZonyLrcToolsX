using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure;
using ZonyLrcTools.Cli.Infrastructure.Album;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;
using ZonyLrcTools.Cli.Infrastructure.Extensions;
using ZonyLrcTools.Cli.Infrastructure.IO;
using ZonyLrcTools.Cli.Infrastructure.Lyric;
using ZonyLrcTools.Cli.Infrastructure.Tag;
using ZonyLrcTools.Cli.Infrastructure.Threading;

namespace ZonyLrcTools.Cli.Commands.SubCommand
{
    [Command("download", Description = "下载歌词文件或专辑图像。")]
    public class DownloadCommand : ToolCommandBase
    {
        private readonly ILogger<DownloadCommand> _logger;
        private readonly IFileScanner _fileScanner;
        private readonly ITagLoader _tagLoader;
        private readonly IEnumerable<ILyricDownloader> _lyricDownloaderList;
        private readonly IEnumerable<IAlbumDownloader> _albumDownloaderList;

        private readonly ToolOptions _options;

        public DownloadCommand(ILogger<DownloadCommand> logger,
            IFileScanner fileScanner,
            IOptions<ToolOptions> options,
            ITagLoader tagLoader,
            IEnumerable<ILyricDownloader> lyricDownloaderList,
            IEnumerable<IAlbumDownloader> albumDownloaderList)
        {
            _logger = logger;
            _fileScanner = fileScanner;
            _tagLoader = tagLoader;
            _lyricDownloaderList = lyricDownloaderList;
            _albumDownloaderList = albumDownloaderList;
            _options = options.Value;
        }

        #region > Options <

        [Option("-d|--dir", CommandOptionType.SingleValue, Description = "指定需要扫描的目录。")]
        [DirectoryExists]
        public string Directory { get; set; }

        [Option("-l|--lyric", CommandOptionType.NoValue, Description = "指定程序需要下载歌词文件。")]
        public bool DownloadLyric { get; set; }

        [Option("-a|--album", CommandOptionType.NoValue, Description = "指定程序需要下载专辑图像。")]
        public bool DownloadAlbum { get; set; }

        [Option("-n|--number", CommandOptionType.SingleValue, Description = "指定下载时候的线程数量。(默认值 2)")]
        public int ParallelNumber { get; set; } = 2;

        #endregion

        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var files = await ScanMusicFilesAsync();
            var musicInfos = await LoadMusicInfoAsync(files);

            if (DownloadLyric)
            {
                await DownloadLyricFilesAsync(musicInfos);
            }

            if (DownloadAlbum)
            {
                await DownloadAlbumAsync(musicInfos);
            }

            return 0;
        }

        private async Task<List<string>> ScanMusicFilesAsync()
        {
            var files = (await _fileScanner.ScanAsync(Directory, _options.SupportFileExtensions))
                .SelectMany(t => t.FilePaths)
                .ToList();

            if (files.Count == 0)
            {
                _logger.LogError("没有找到任何音乐文件。");
                throw new ErrorCodeException(ErrorCodes.NoFilesWereScanned);
            }

            files = RemoveExistLyricFiles(files);

            _logger.LogInformation($"已经扫描到了 {files.Count} 个音乐文件。");

            return files;
        }

        private List<string> RemoveExistLyricFiles(List<string> filePaths)
        {
            if (!_options.Provider.Lyric.Config.IsSkipExistLyricFiles)
            {
                return filePaths;
            }

            return filePaths
                .Where(path =>
                {
                    if (!File.Exists(Path.ChangeExtension(path, ".lrc")))
                    {
                        return true;
                    }

                    _logger.LogWarning($"已经存在歌词文件 {path}，跳过。");
                    return false;
                })
                .ToList();
        }

        private async Task<ImmutableList<MusicInfo>> LoadMusicInfoAsync(IReadOnlyCollection<string> files)
        {
            _logger.LogInformation("开始加载音乐文件的标签信息...");

            var warpTask = new WarpTask(ParallelNumber);
            var warpTaskList = files.Select(file => warpTask.RunAsync(() => Task.Run(async () => await _tagLoader.LoadTagAsync(file))));
            var result = (await Task.WhenAll(warpTaskList))
                .Where(m => m != null)
                .Where(m => !string.IsNullOrEmpty(m.Name) || !string.IsNullOrEmpty(m.Artist));

            _logger.LogInformation($"已成功加载 {files.Count} 个音乐文件的标签信息。");

            return result.ToImmutableList();
        }

        private IEnumerable<ILyricDownloader> GetLyricDownloaderList()
        {
            var downloader = _options.Provider.Lyric.Plugin
                .Where(op => op.Priority != -1)
                .OrderBy(op => op.Priority)
                .Join(_lyricDownloaderList,
                    op => op.Name,
                    loader => loader.DownloaderName,
                    (op, loader) => loader);

            return downloader;
        }

        #region > Lyric download logic <

        private async ValueTask DownloadLyricFilesAsync(ImmutableList<MusicInfo> musicInfos)
        {
            _logger.LogInformation("开始下载歌词文件数据...");

            var downloaderList = GetLyricDownloaderList();
            var warpTask = new WarpTask(ParallelNumber);
            var warpTaskList = musicInfos.Select(info =>
                warpTask.RunAsync(() => Task.Run(async () => await DownloadLyricTaskLogicAsync(downloaderList, info))));

            await Task.WhenAll(warpTaskList);

            _logger.LogInformation($"歌词数据下载完成，成功: {musicInfos.Count} 失败{musicInfos.Count - musicInfos.Count(m => m.IsSuccessful)}。");
        }

        private async Task DownloadLyricTaskLogicAsync(IEnumerable<ILyricDownloader> downloaderList, MusicInfo info)
        {
            async Task InternalDownloadLogicAsync(ILyricDownloader downloader)
            {
                try
                {
                    var lyric = await downloader.DownloadAsync(info.Name, info.Artist);
                    var lyricFilePath = Path.Combine(Path.GetDirectoryName(info.FilePath)!,
                        $"{Path.GetFileNameWithoutExtension(info.FilePath)}.lrc");

                    if (File.Exists(lyricFilePath))
                    {
                        File.Delete(lyricFilePath);
                    }

                    if (lyric.IsPruneMusic)
                    {
                        info.IsSuccessful = true;
                    }

                    await using var stream = new FileStream(lyricFilePath, FileMode.Create);
                    await using var sw = new StreamWriter(stream);
                    await sw.WriteAsync(lyric.ToString());
                    await sw.FlushAsync();
                }
                catch (ErrorCodeException ex)
                {
                    if (ex.ErrorCode == ErrorCodes.NoMatchingSong)
                    {
                        info.IsSuccessful = false;
                    }

                    _logger.LogWarningInfo(ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"下载歌词文件时发生错误：{ex.Message}，歌曲名: {info.Name}，歌手: {info.Artist}。");
                    info.IsSuccessful = false;
                }
                finally
                {
                    info.IsSuccessful = true;
                }
            }

            foreach (var downloader in downloaderList)
            {
                await InternalDownloadLogicAsync(downloader);

                if (info.IsSuccessful)
                {
                    _logger.LogSuccessful(info);
                    return;
                }
            }
        }

        #endregion

        #region > Ablum image download logic <

        private async ValueTask DownloadAlbumAsync(ImmutableList<MusicInfo> musicInfos)
        {
            _logger.LogInformation("开始下载专辑图像数据...");

            var downloader = _albumDownloaderList.FirstOrDefault(d => d.DownloaderName == InternalAlbumDownloaderNames.NetEase);
            var warpTask = new WarpTask(ParallelNumber);
            var warpTaskList = musicInfos.Select(info =>
                warpTask.RunAsync(() => Task.Run(async () => await DownloadAlbumTaskLogicAsync(downloader, info))));

            await Task.WhenAll(warpTaskList);

            _logger.LogInformation($"专辑数据下载完成，成功: {musicInfos.Count(m => m.IsSuccessful)} 失败{musicInfos.Count(m => !m.IsSuccessful)}。");
        }

        private async Task DownloadAlbumTaskLogicAsync(IAlbumDownloader downloader, MusicInfo info)
        {
            _logger.LogSuccessful(info);

            try
            {
                var album = await downloader.DownloadAsync(info.Name, info.Artist);
                var filePath = Path.Combine(Path.GetDirectoryName(info.FilePath)!, $"{Path.GetFileNameWithoutExtension(info.FilePath)}.png");
                if (File.Exists(filePath) || album.Length <= 0)
                {
                    return;
                }

                await new FileStream(filePath, FileMode.Create).WriteBytesToFileAsync(album, 1024);
            }
            catch (ErrorCodeException ex)
            {
                _logger.LogWarningInfo(ex);
            }
        }

        #endregion
    }
}