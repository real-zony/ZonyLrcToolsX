using System;
using System.Reflection;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using ZonyLrcTools.Common;
using ZonyLrcTools.Forms;

namespace ZonyLrcTools
{
    static class Program
    {
        /// <summary>
        /// 应用程序入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitializeIocContainer();

            // 全局异常捕获处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (object sender, System.Threading.ThreadExceptionEventArgs e) => Log4NetHelper.Exception(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => Log4NetHelper.Exception(e.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(IocManager.Instance.Resolve<Form_Startup>());
        }

        /// <summary>
        /// 初始化 IOC 容器
        /// </summary>
        private static void InitializeIocContainer()
        {
            EventBus.Init();
            IocManager.Instance.AddConventionalRegistrar(new BasicConventionalRegistrar());
            IocManager.Instance.RegisterAssemblyByConvention(Assembly.LoadFrom(Environment.CurrentDirectory + "\\Zony.Lib.Plugin.dll"));

            IocManager.Instance.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly(), new ConventionalRegistrationConfig()
            {
                InstallInstallers = false
            });
        }
    }
}