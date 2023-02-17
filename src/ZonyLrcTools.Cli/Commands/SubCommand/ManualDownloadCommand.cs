using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace ZonyLrcTools.Cli.Commands.SubCommand;

[Command("manual", Description = "手动指定歌曲信息，然后下载对应的歌词数据。")]
public class ManualDownloadCommand : ToolCommandBase
{
    private readonly ILogger<ManualDownloadCommand> _logger;

    public ManualDownloadCommand(ILogger<ManualDownloadCommand> logger)
    {
        _logger = logger;
    }
}