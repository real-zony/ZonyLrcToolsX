using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using MusicDecrypto.Library;
using ZonyLrcTools.Common.Infrastructure.IO;
using ZonyLrcTools.Common.Infrastructure.Threading;
using ZonyLrcTools.Common.MusicDecryption;

namespace ZonyLrcTools.Cli.Commands.SubCommand
{
    /// <summary>
    /// 工具类相关命令。
    /// </summary>
    [Command("util", Description = "提供常用的工具类功能。")]
    public class UtilityCommand : ToolCommandBase
    {
        private readonly ILogger<UtilityCommand> _logger;
        private readonly IMusicDecryptor _musicDecryptor;

        [Required(ErrorMessage = "请指定需要解密的歌曲文件或文件夹路径。")]
        [Option("-s|--source", CommandOptionType.SingleValue, Description = "需要解密的歌曲文件或文件夹路径。", ShowInHelpText = true)]
        public string Source { get; set; }

        private readonly IFileScanner _fileScanner;

        public UtilityCommand(IFileScanner fileScanner,
            ILogger<UtilityCommand> logger,
            IMusicDecryptor musicDecryptor)
        {
            _fileScanner = fileScanner;
            _logger = logger;
            _musicDecryptor = musicDecryptor;
        }

        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (Directory.Exists(Source))
            {
                _logger.LogInformation("开始扫描文件夹，请稍等...");

                var files = (await _fileScanner.ScanAsync(Source, DecryptoFactory.KnownExtensions.Select(x => $"*{x}")))
                    .SelectMany(f => f.FilePaths)
                    .ToList();

                _logger.LogInformation($"扫描完成，共 {files.Count} 个文件，准备转换。");

                var wrapTask = new WarpTask(4);
                var tasks = files.Select(path => wrapTask.RunAsync(async () =>
                {
                    _logger.LogInformation($"开始转换文件：{path}");
                    var result = await _musicDecryptor.ConvertMusicAsync(path);
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation($"转换完成，文件保存在：{result.OutputFilePath}");
                    }
                    else
                    {
                        _logger.LogError($"转换失败，原因：{result.ErrorMessage}");
                    }
                }));

                await Task.WhenAll(tasks);
            }
            else if (File.Exists(Source))
            {
                await _musicDecryptor.ConvertMusicAsync(Source);
            }

            _logger.LogInformation("所有文件已经转换完成...");

            return 0;
        }
    }
}