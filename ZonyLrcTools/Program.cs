using System;
using System.Windows.Forms;
using ZonyLrcTools.Forms;
using Castle.Windsor;

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
            Application.Run(new Form_Startup());
        }

        private static void bootStarpper()
        {
            IWindsorContainer _container = new WindsorContainer();
        }
    }
}
