using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace ZonyLrcTools.Cli.Commands
{
    [HelpOption("--help|-h", Description = "欢迎使用 ZonyLrcToolsX Command Line Interface。")]
    public abstract class ToolCommandBase
    {
        protected virtual Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            return Task.FromResult(0);
        }
    }
}