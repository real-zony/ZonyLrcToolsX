using Castle.Windsor;
using System;

namespace Zony.Lib.Infrastructures.Dependency
{
    /// <summary>
    /// IOC 容器
    /// </summary>
    /// <remarks>这里遵循了接口隔离原则</remarks>
    public interface IIocManager : IIocRegistrar, IIocResolver, IDisposable
    {
        IWindsorContainer IocContainer { get; }
        new bool IsRegistered(Type type);
        new bool IsRegistered<T>();
    }
}