using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zony.Lib.Infrastructures.Dependency
{
    public class IocManager : IIocManager
    {
        /// <summary>
        /// Ioc 单例容器
        /// </summary>
        public static IocManager Instance { get; private set; }
        /// <summary>
        /// Windsor 容器
        /// </summary>
        public IWindsorContainer IocContainer { get; private set; }

        /// <summary>
        /// 注册约定列表
        /// </summary>
        private readonly List<IConventionalDependencyRegistrar> _conventionalRegistrar;

        static IocManager() => Instance = new IocManager();

        public IocManager()
        {
            IocContainer = new WindsorContainer();
            _conventionalRegistrar = new List<IConventionalDependencyRegistrar>();

            // 注册自身
            IocContainer.Register(Component.For<IocManager, IIocManager, IIocRegistrar, IIocResolver>().UsingFactoryMethod(() => this));
        }

        public void AddConventionalRegistrar(IConventionalDependencyRegistrar registrar)
        {
            _conventionalRegistrar.Add(registrar);
        }

        public void Dispose()
        {
            IocContainer.Dispose();
        }

        public bool IsRegistered(Type type)
        {
            return IocContainer.Kernel.HasComponent(type);
        }

        public bool IsRegistered<T>()
        {
            return IocContainer.Kernel.HasComponent(typeof(T));
        }

        public void Register<T>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton) where T : class
        {
            IocContainer.Register(applyLifestyle(Component.For<T>(), lifeStyle));
        }

        public void Register(Type type, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
        {
            IocContainer.Register(applyLifestyle(Component.For(type), lifeStyle));
        }

        public void Register<TType, TImpl>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
    where TType : class
    where TImpl : class, TType
        {
            IocContainer.Register(applyLifestyle(Component.For<TType, TImpl>().ImplementedBy<TImpl>(), lifeStyle));
        }

        public void Register(Type type, Type impl, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
        {
            IocContainer.Register(applyLifestyle(Component.For(type, impl).ImplementedBy(impl), lifeStyle));
        }

        public void RegisterAssemblyByConvention(Assembly assembly)
        {
            RegisterAssemblyByConvention(assembly, new ConventionalRegistrationConfig());
        }

        public void RegisterAssemblyByConvention(Assembly assembly, ConventionalRegistrationConfig config)
        {
            var _context = new ConventionalRegistrationContext(assembly, this, config);
            foreach (var registerer in _conventionalRegistrar)
            {
                registerer.RegisterAssembly(_context);
            }

            if (config.InstallInstallers) IocContainer.Install(FromAssembly.Instance(assembly));
        }

        public void Release(object obj)
        {
            IocContainer.Release(obj);
        }

        public T Resolve<T>()
        {
            return IocContainer.Resolve<T>();
        }

        public T Resolve<T>(Type type)
        {
            return (T)IocContainer.Resolve(type);
        }

        public T Resolve<T>(object argumentsAsAnonymousType)
        {
            return IocContainer.Resolve<T>(argumentsAsAnonymousType);
        }

        public object Resolve(Type type)
        {
            return IocContainer.Resolve(type);
        }

        public object Resolve(Type type, object argumentsAsAnonymousType)
        {
            return IocContainer.Resolve(type, argumentsAsAnonymousType);
        }

        public T[] ResolveAll<T>()
        {
            return IocContainer.ResolveAll<T>();
        }

        public T[] ResolveAll<T>(object argumentsAsAnonymousType)
        {
            return IocContainer.ResolveAll<T>(argumentsAsAnonymousType);
        }

        public object[] ResolveAll(Type type)
        {
            return IocContainer.ResolveAll(type).Cast<object>().ToArray();
        }

        public object[] ResolveAll(Type type, object argumentsAsAnonymousType)
        {
            return IocContainer.ResolveAll(type).Cast<object>().ToArray();
        }

        private static ComponentRegistration<T> applyLifestyle<T>(ComponentRegistration<T> registration, DependencyLifeStyle lifeStyle) where T : class
        {
            switch (lifeStyle)
            {
                case DependencyLifeStyle.Transient:
                    return registration.LifestyleTransient();
                case DependencyLifeStyle.Singleton:
                    return registration.LifestyleSingleton();
                default:
                    return registration;
            }
        }
    }
}