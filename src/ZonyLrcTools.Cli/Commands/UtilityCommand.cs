using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using ZonyLrcTools.Cli.Infrastructure.IO;
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
        [Required(ErrorMessage = "音乐格式为必须参数，请指定 -t 参数。")]
        [Option("-t|--type", CommandOptionType.SingleValue, Description = "需要转换的文件格式，参数[Ncm、Qcm]。", ShowInHelpText = true)]
        public SupportFileType Type { get; set; }

        [Required(ErrorMessage = "文件路径为必须按参数，请传入有效路径。")]
        [Argument(0, "Path", "指定需要转换的音乐文件路径，支持目录和文件路径。")]
        public string Path { get; set; }

        private readonly IFileScanner _fileScanner;

        public UtilityCommand(IFileScanner fileScanner)
        {
            _fileScanner = fileScanner;
        }

        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (Directory.Exists(Path))
            {
                var files = await _fileScanner.ScanAsync(Path, new[] {"*.ncm"});

                var wrapTask = new WarpTask(4);
                var tasks = files
                    .SelectMany(f => f.FilePaths)
                    .Select(path => wrapTask.RunAsync(() => Convert(path)));

                await Task.WhenAll(tasks);
            }
            else if (File.Exists(Path))
            {
                await Convert(Path);
            }

            return 0;
        }

        private async Task Convert(string filePath)
        {
            await Task.CompletedTask;
        }
    }
}