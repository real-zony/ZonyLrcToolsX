using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Album;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Extensions;
using ZonyLrcTools.Common.Infrastructure.IO;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Threading;
using ZonyLrcTools.Common.Lyrics;

namespace ZonyLrcTools.Cli.Commands.SubCommand
{
    [Command("download", Description = "下载歌词文件或专辑图像。")]
    public class DownloadCommand : ToolCommandBase
    {
        private readonly ILyricsDownloader _lyricsDownloader;
        private readonly IMusicInfoLoader _musicInfoLoader;
        private readonly IEnumerable<IAlbumDownloader> _albumDownloaderList;

        private readonly IWarpLogger _logger;
        private readonly GlobalOptions _options;

        public DownloadCommand(IOptions<GlobalOptions> options,
            IEnumerable<IAlbumDownloader> albumDownloaderList,
            ILyricsDownloader lyricsDownloader,
            IWarpLogger logger,
            IMusicInfoLoader musicInfoLoader)
        {
            _albumDownloaderList = albumDownloaderList;
            _lyricsDownloader = lyricsDownloader;
            _logger = logger;
            _musicInfoLoader = musicInfoLoader;
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
                await _lyricsDownloader.DownloadAsync(await _musicInfoLoader.LoadAsync(SongsDirectory, ParallelNumber), ParallelNumber);
            }

            if (DownloadAlbum)
            {
                // await DownloadAlbumAsync(
                //     await LoadMusicInfoAsync(
                //         await ScanMusicFilesAsync()));
            }

            return 0;
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