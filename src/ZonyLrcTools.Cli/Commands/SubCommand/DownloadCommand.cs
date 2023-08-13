using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Cli.Infrastructure.MusicScannerOptions;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Album;
using ZonyLrcTools.Common.Lyrics;
using ZonyLrcTools.Common.MusicScanner;

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
        private readonly IServiceProvider _serviceProvider;

        public DownloadCommand(ILyricsDownloader lyricsDownloader,
            IMusicInfoLoader musicInfoLoader,
            IAlbumDownloader albumDownloader,
            IServiceProvider serviceProvider)
        {
            _lyricsDownloader = lyricsDownloader;
            _musicInfoLoader = musicInfoLoader;
            _albumDownloader = albumDownloader;
            _serviceProvider = serviceProvider;
        }

        #region > Options <

        [Option("-d|--dir", CommandOptionType.SingleValue, Description = "指定需要扫描的目录。")]
        [DirectoryExists]
        public string SongsDirectory { get; set; }

        [Option("-l|--lyric", CommandOptionType.NoValue, Description = "指定程序需要下载歌词文件。")]
        public bool DownloadLyric { get; set; }

        [Option("-a|--album", CommandOptionType.NoValue, Description = "指定程序需要下载专辑图像。")]
        public bool DownloadAlbum { get; set; }

        [Option("-n|--number", CommandOptionType.SingleValue, Description = "指定下载时候的线程数量。(默认值 1)")]
        public int ParallelNumber { get; set; } = 1;

        #endregion

        #region > Scanner Options <

        [Option("-sc|--scanner", CommandOptionType.SingleValue, Description = "指定歌词文件扫描器，目前支持本地文件(local)，网易云音乐(netease)，csv 文件(csv)，默认值为 local。")]
        public string Scanner { get; set; } = "local";

        [Option("-o|--output", Description = "指定歌词文件的输出路径。")]
        public string OutputDirectory { get; set; } = "DownloadedLrc";

        [Option("-p|--pattern", Description = "指定歌词文件的输出文件名模式。")]
        public string OutputFileNamePattern { get; set; } = "{Artist} - {Name}.lrc";

        [Option("-f|--file", Description = "指定 CSV 文件的路径。")]
        public string CsvFilePath { get; set; }

        [Option("-s|--song-list-id", Description = "指定网易云音乐歌单的 ID，如果有多个歌单，请使用 ';' 分割 ID。")]
        public string SongListId { get; set; }

        #endregion

        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (!DownloadAlbum && !DownloadLyric)
            {
                throw new ArgumentException("请至少指定一个下载选项，例如 -l(下载歌词) 或 -a(下载专辑图像)。");
            }

            if (DownloadLyric)
            {
                await _lyricsDownloader.DownloadAsync(await GetMusicInfosAsync(Scanner), ParallelNumber);
            }

            if (DownloadAlbum)
            {
                await _albumDownloader.DownloadAsync(await GetMusicInfosAsync(Scanner), ParallelNumber);
            }

            return 0;
        }

        /// <summary>
        /// Get the music infos by the scanner.
        /// </summary>
        private async Task<List<MusicInfo>> GetMusicInfosAsync(string scanner)
        {
            ValidateScannerOptions(scanner);

            return scanner switch
            {
                MusicScannerConsts.LocalScanner => await _musicInfoLoader.LoadAsync(SongsDirectory, ParallelNumber),
                MusicScannerConsts.CsvScanner => await _serviceProvider.GetService<CsvFileMusicScanner>()
                    .GetMusicInfoFromCsvFileAsync(CsvFilePath, OutputDirectory, OutputFileNamePattern),
                MusicScannerConsts.NeteaseScanner => await _serviceProvider.GetService<NetEaseMusicSongListMusicScanner>()
                    .GetMusicInfoFromNetEaseMusicSongListAsync(SongListId, OutputDirectory, OutputFileNamePattern),
                _ => await _musicInfoLoader.LoadAsync(SongsDirectory, ParallelNumber)
            };
        }

        /// <summary>
        /// Manually validate the options.
        /// </summary>
        /// <param name="scanner">Scanner Name.</param>
        /// <exception cref="ArgumentException">If the options are invalid.</exception>
        private void ValidateScannerOptions(string scanner)
        {
            if (scanner != MusicScannerConsts.LocalScanner && string.IsNullOrEmpty(OutputDirectory))
            {
                throw new ArgumentException("当使用非本地文件扫描器时，必须指定歌词文件的输出路径。");
            }

            if (scanner != MusicScannerConsts.LocalScanner && !Directory.Exists(OutputDirectory))
            {
                throw new ArgumentException("指定的歌词文件输出路径不存在。");
            }

            switch (scanner)
            {
                case MusicScannerConsts.CsvScanner when string.IsNullOrWhiteSpace(CsvFilePath):
                    throw new ArgumentException("当使用 CSV 文件扫描器时，必须指定 CSV 文件的路径。");
                case MusicScannerConsts.NeteaseScanner when string.IsNullOrWhiteSpace(SongListId):
                    throw new ArgumentException("当使用网易云音乐扫描器时，必须指定歌单的 ID。");
            }
        }
    }
}