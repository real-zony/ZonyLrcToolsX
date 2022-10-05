using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace ZonyLrcTools.Cli.Commands.SubCommand;

[Command("search", Description = "手动指定信息，用于搜索歌词数据。")]
public class SearchCommand : ToolCommandBase
{
    private readonly ILogger<SearchCommand> _logger;

    #region > Options <

    public string Name { get; set; }

    public string Artist { get; set; }

    #endregion

    public SearchCommand(ILogger<SearchCommand> logger)
    {
        _logger = logger;
    }

    protected override Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        return base.OnExecuteAsync(app);
    }
}