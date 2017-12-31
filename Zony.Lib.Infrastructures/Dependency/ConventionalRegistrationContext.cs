using System.Reflection;

namespace Zony.Lib.Infrastructures.Dependency
{
    /// <summary>
    /// 注册约束上下文
    /// </summary>
    public class ConventionalRegistrationContext : IConventionalRegistrationContext
    {
        /// <summary>
        /// 绑定的程序集
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Ioc 容器
        /// </summary>
        public IIocManager IocManager { get; private set; }
        /// <summary>
        /// 绑定的配置项
        /// </summary>
        public ConventionalRegistrationConfig Config { get; set; }

        /// <summary>
        /// 注册约束上下文
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="iocManager">Ioc 容器</param>
        /// <param name="config">配置项</param>
        public ConventionalRegistrationContext(Assembly assembly, IIocManager iocManager, ConventionalRegistrationConfig config)
        {
            Assembly = assembly;
            IocManager = iocManager;
            Config = config;
        }
    }
}
