using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Infrastructure.Tag;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Album;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Extensions;
using ZonyLrcTools.Common.Infrastructure.IO;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Threading;
using ZonyLrcTools.Common.Lyrics;
using File = System.IO.File;

namespace ZonyLrcTools.Cli.Commands.SubCommand
{
    [Command("download", Description = "下载歌词文件或专辑图像。")]
    public class DownloadCommand : ToolCommandBase
    {
        private readonly ILyricsDownloader _lyricsDownloader;
        private readonly IFileScanner _fileScanner;
        private readonly IEnumerable<IAlbumDownloader> _albumDownloaderList;
        private readonly ITagLoader _tagLoader;
        private readonly IWarpLogger _logger;

        private readonly GlobalOptions _options;

        public DownloadCommand(IFileScanner fileScanner,
            IOptions<GlobalOptions> options,
            IEnumerable<IAlbumDownloader> albumDownloaderList,
            ITagLoader tagLoader,
            ILyricsDownloader lyricsDownloader, IWarpLogger logger)
        {
            _fileScanner = fileScanner;
            _albumDownloaderList = albumDownloaderList;
            _tagLoader = tagLoader;
            _lyricsDownloader = lyricsDownloader;
            _logger = logger;
            _options = options.Value;
        }

        #region > Options <

        [Option("-d|--dir", CommandOptionType.SingleValue, Description = "指定需要扫描的目录。")]
        [DirectoryExists]
        public string SongsDirectory { get; set; }

        [Option("-l|--lyric", CommandOptionType.NoValue, Description = "指定程序需要下载歌词文件。")]
        public bool DownloadLyric { get; set; }

        [Option("-a|--album", CommandOptionType.NoValue, Description = "指定程序需要下载专辑图像。")]
        public bool DownloadAlbum { get; set; }

        [Option("-n|--number", CommandOptionType.SingleValue, Description = "指定下载时候的线程数量。(默认值 2)")]
        public int ParallelNumber { get; set; } = 2;

        #endregion

        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (DownloadLyric)
            {
                await _lyricsDownloader.DownloadAsync(
                    await LoadMusicInfoAsync(
                        RemoveExistLyricFiles(
                            await ScanMusicFilesAsync())), ParallelNumber);
            }

            if (DownloadAlbum)
            {
                await DownloadAlbumAsync(
                    await LoadMusicInfoAsync(
                        await ScanMusicFilesAsync()));
            }

            return 0;
        }

        private async Task<List<string>> ScanMusicFilesAsync()
        {
            var files = (await _fileScanner.ScanAsync(SongsDirectory, _options.SupportFileExtensions))
                .SelectMany(t => t.FilePaths)
                .ToList();

            if (files.Count == 0)
            {
                await _logger.ErrorAsync("没有找到任何音乐文件。");
                throw new ErrorCodeException(ErrorCodes.NoFilesWereScanned);
            }

            await _logger.InfoAsync($"已经扫描到了 {files.Count} 个音乐文件。");

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

                    _logger.WarnAsync($"已经存在歌词文件 {path}，跳过。").GetAwaiter().GetResult();
                    return false;
                })
                .ToList();
        }

        private async Task<List<MusicInfo>> LoadMusicInfoAsync(IReadOnlyCollection<string> files)
        {
            await _logger.InfoAsync("开始加载音乐文件的标签信息...");

            var warpTask = new WarpTask(ParallelNumber);
            var warpTaskList = files.Select(file => warpTask.RunAsync(() => Task.Run(async () => await _tagLoader.LoadTagAsync(file))));
            var result = (await Task.WhenAll(warpTaskList))
                .Where(m => m != null)
                .Where(m => !string.IsNullOrEmpty(m.Name) || !string.IsNullOrEmpty(m.Artist))
                .ToList();

            await _logger.InfoAsync($"已成功加载 {files.Count} 个音乐文件的标签信息。");
            return result;
        }

        #region > Ablum image download logic <

        private async ValueTask DownloadAlbumAsync(List<MusicInfo> musicInfos)
        {
            await _logger.InfoAsync("开始下载专辑图像数据...");

            var downloader = _albumDownloaderList.FirstOrDefault(d => d.DownloaderName == InternalAlbumDownloaderNames.NetEase);
            var warpTask = new WarpTask(ParallelNumber);
            var warpTaskList = musicInfos.Select(info =>
                warpTask.RunAsync(() => Task.Run(async () => await DownloadAlbumTaskLogicAsync(downloader, info))));

            await Task.WhenAll(warpTaskList);

            await _logger.InfoAsync($"专辑数据下载完成，成功: {musicInfos.Count(m => m.IsSuccessful)} 失败{musicInfos.Count(m => m.IsSuccessful == false)}。");
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