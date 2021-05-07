using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.IO;

namespace ZonyLrcTools.Cli.Commands
{
    [Command("scan", Description = "扫描指定目录下符合条件的音乐文件数量。")]
    public class ScanCommand : ToolCommandBase
    {
        private readonly IFileScanner _fileScanner;
        private readonly ToolOptions _options;
        private readonly ILogger<ScanCommand> _logger;

        public ScanCommand(IFileScanner fileScanner,
            IOptions<ToolOptions> options,
            ILogger<ScanCommand> logger)
        {
            _fileScanner = fileScanner;
            _logger = logger;
            _options = options.Value;
        }

        [Option("-d|--dir", CommandOptionType.SingleValue, Description = "指定需要扫描的目录。")]
        [DirectoryExists]
        public string DirectoryPath { get; set; }

        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var result = await _fileScanner.ScanAsync(
                DirectoryPath,
                _options.SupportFileExtensions.Split(';'));

            _logger.LogInformation($"目录扫描完成，共扫描到 {result.Sum(f => f.FilePaths.Count)} 个音乐文件。");
            return 0;
        }

        public override List<string> CreateArgs()
        {
            return new();
        }
    }
}