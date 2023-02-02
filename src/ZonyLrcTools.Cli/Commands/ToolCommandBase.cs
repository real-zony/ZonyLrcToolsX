using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace ZonyLrcTools.Cli.Commands
{
    [HelpOption("--help|-h", Description = "欢迎使用 ZonyLrcToolsX Command Line Interface，有任何问题请访问 https://soft.myzony.com 或添加 QQ 群 337656932 寻求帮助。")]
    public abstract class ToolCommandBase
    {
        protected virtual Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            return Task.FromResult(0);
        }
    }
}