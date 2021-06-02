using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;
using ZonyLrcTools.Cli.Infrastructure.IO;
using ZonyLrcTools.Cli.Infrastructure.MusicDecryption;
using ZonyLrcTools.Cli.Infrastructure.Threading;

namespace ZonyLrcTools.Cli.Commands
{
    public enum SupportFileType
    {
        Ncm = 1,
        Qcm = 2
    }

    /// <summary>
    /// 工具类相关命令。
    /// </summary>
    [Command("util", Description = "提供常用的工具类功能。")]
    public class UtilityCommand : ToolCommandBase
    {
        private readonly ILogger<UtilityCommand> _logger;
        private readonly IMusicDecryptor _musicDecryptor;

        [Required(ErrorMessage = "音乐格式为必须参数，请指定 -t 参数。")]
        [Option("-t|--type", CommandOptionType.SingleValue, Description = "需要转换的文件格式，参数[Ncm、Qcm]。", ShowInHelpText = true)]
        public SupportFileType Type { get; set; }

        [Required(ErrorMessage = "文件路径为必须按参数，请传入有效路径。")]
        [Argument(0, "FilePath", "指定需要转换的音乐文件路径，支持目录和文件路径。")]
        public string FilePath { get; set; }

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
            if (Directory.Exists(FilePath))
            {
                _logger.LogInformation("开始扫描文件夹，请稍等...");

                var files = (await _fileScanner.ScanAsync(FilePath, new[] {"*.ncm"}))
                    .SelectMany(f => f.FilePaths)
                    .ToList();

                _logger.LogInformation($"扫描完成，共 {files.Count} 个文件，准备转换。");

                var wrapTask = new WarpTask(4);
                var tasks = files.Select(path => wrapTask.RunAsync(() => Convert(path)));

                await Task.WhenAll(tasks);
            }
            else if (File.Exists(FilePath))
            {
                await Convert(FilePath);
            }

            _logger.LogInformation("所有文件已经转换完成...");

            return 0;
        }

        private async Task Convert(string filePath)
        {
            if (Type != SupportFileType.Ncm)
            {
                throw new ErrorCodeException(ErrorCodes.OnlySupportNcmFormatFile);
            }

            var memoryStream = new MemoryStream();
            await using var file = File.Open(filePath, FileMode.Open);
            {
                var buffer = new Memory<byte>(new byte[2048]);
                while (await file.ReadAsync(buffer) > 0)
                {
                    await memoryStream.WriteAsync(buffer);
                }
            }

            var result = await _musicDecryptor.ConvertMusic(memoryStream.ToArray());
            var newFileName = Path.Combine(Path.GetDirectoryName(filePath),
                $"{Path.GetFileNameWithoutExtension(filePath)}.{((JObject) result.ExtensionObjects["JSON"]).SelectToken("$.format").Value<string>()}");

            await using var musicFileStream = File.Create(newFileName);
            await musicFileStream.WriteAsync(result.Data);
            await musicFileStream.FlushAsync();
        }
    }
}