using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace ZonyLrcTools.Cli.Commands
{
    [HelpOption("--help|-h",
        Description = "欢迎使用 ZonyLrcToolsX Command Line Interface，有任何问题请访问 https://soft.myzony.com 或添加 QQ 群 337656932 寻求帮助。",
        ShowInHelpText = true)]
    public abstract class ToolCommandBase
    {
        protected virtual Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (!Environment.UserInteractive)
            {
                Console.WriteLine("请使用终端运行此程序，如果你不知道如何操作，请访问 https://soft.myzony.com 或添加 QQ 群 337656932 寻求帮助。");
                Console.ReadKey();
            }

            app.ShowHelp();
            return Task.FromResult(0);
        }
    }
}