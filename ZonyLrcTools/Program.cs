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
            bootStarpper();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(IocManager.Instance.Resolve<Form_Startup>());
        }

        private static void bootStarpper()
        {
            EventBus.Init();
            IocManager.Instance.AddConventionalRegistrar(new BasicConventionalRegistrar());
            IocManager.Instance.RegisterAssemblyByConvention(Assembly.LoadFrom(Environment.CurrentDirectory + "\\Zony.Lib.Plugin.dll"));
            IocManager.Instance.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly(), new ConventionalRegistrationConfig()
            {
                InstallInstallers = false
            });
            //IocManager.Instance.RegisterAssemblyByConvention(Assembly.LoadFrom(Environment.CurrentDirectory + "\\Plugins\\Zony.Lib.TagLib.dll"));
        }
    }
}
