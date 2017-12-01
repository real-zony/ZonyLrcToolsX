using System.Reflection;

namespace Zony.Lib.Infrastructures.Dependency
{
    public class ConventionalRegistrationContext : IConventionalRegistrationContext
    {
        public Assembly Assembly { get; private set; }

        public IIocManager IocManager { get; private set; }
        public ConventionalRegistrationConfig Config { get; set; }

        public ConventionalRegistrationContext(Assembly assembly, IIocManager iocManager, ConventionalRegistrationConfig config)
        {
            Assembly = assembly;
            IocManager = iocManager;
            Config = config;
        }
    }
}
