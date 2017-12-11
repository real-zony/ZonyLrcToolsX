using System;
using System.Reflection;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using ZonyLrcTools.Forms;

namespace ZonyLrcTools
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitializeIocContainer();

            // 全局异常捕获处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Log4Net 初始化
            InitializeLog4Net();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(IocManager.Instance.Resolve<Form_Startup>());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {

        }

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

        private static void InitializeLog4Net()
        {

        }
    }
}
