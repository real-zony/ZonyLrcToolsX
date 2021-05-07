using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace ZonyLrcTools.Cli.Commands
{
    /// <summary>
    /// 工具类相关命令。
    /// </summary>
    [Command("util", Description = "提供常用的工具类功能。")]
    public class UtilityCommand : ToolCommandBase
    {
        public override List<string> CreateArgs() => new();

        protected override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            return base.OnExecuteAsync(app);
        }
    }
}