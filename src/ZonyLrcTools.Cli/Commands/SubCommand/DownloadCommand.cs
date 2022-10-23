using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Album;
using ZonyLrcTools.Common.Lyrics;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable MemberCanBePrivate.Global

namespace ZonyLrcTools.Cli.Commands.SubCommand
{
    [Command("download", Description = "下载歌词文件或专辑图像。")]
    public class DownloadCommand : ToolCommandBase
    {
        private readonly ILyricsDownloader _lyricsDownloader;
        private readonly IAlbumDownloader _albumDownloader;
        private readonly IMusicInfoLoader _musicInfoLoader;

        public DownloadCommand(ILyricsDownloader lyricsDownloader,
            IMusicInfoLoader musicInfoLoader,
            IAlbumDownloader albumDownloader)
        {
            _lyricsDownloader = lyricsDownloader;
            _musicInfoLoader = musicInfoLoader;
            _albumDownloader = albumDownloader;
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
                await _albumDownloader.DownloadAsync(await _musicInfoLoader.LoadAsync(SongsDirectory, ParallelNumber), ParallelNumber);
            }

            return 0;
        }
    }
}