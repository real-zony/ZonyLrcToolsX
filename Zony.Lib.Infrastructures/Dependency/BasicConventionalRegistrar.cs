using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using System.Reflection;

namespace Zony.Lib.Infrastructures.Dependency
{
    /// <summary>
    ///  瞬态对象
    /// </summary>
    public interface ITransientDependency { }

    /// <summary>
    /// 单件对象
    /// </summary>
    public interface ISingletonDependency { }

    public class BasicConventionalRegistrar : IConventionalDependencyRegistrar
    {
        public void RegisterAssembly(IConventionalRegistrationContext context)
        {
            // 注册瞬态对象
            context.IocManager.IocContainer.Register(Classes.FromAssembly(context.Assembly)
                                                            .IncludeNonPublicTypes()
                                                            .BasedOn<ITransientDependency>()
                                                            .If(type => !type.GetTypeInfo().IsConstructedGenericType)
                                                            .WithService.Self()
                                                            .WithService.DefaultInterfaces()
                                                            .LifestyleTransient());

            // 注册单例对象
            context.IocManager.IocContainer.Register(Classes.FromAssembly(context.Assembly)
                                                            .IncludeNonPublicTypes()
                                                            .BasedOn<ISingletonDependency>()
                                                            .If(type => !type.GetTypeInfo().IsConstructedGenericType)
                                                            .WithService.Self()
                                                            .WithService.DefaultInterfaces()
                                                            .LifestyleSingleton());

            // 注册拦截器
            context.IocManager.IocContainer.Register(Classes.FromAssembly(context.Assembly)
                                                            .IncludeNonPublicTypes()
                                                            .BasedOn<IInterceptor>()
                                                            .If(type => !type.GetTypeInfo().IsConstructedGenericType)
                                                            .WithService.Self()
                                                            .LifestyleTransient());
        }
    }
}