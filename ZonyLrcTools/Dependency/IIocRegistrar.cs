using System;
using System.Reflection;

namespace ZonyLrcTools.Dependency
{
    public interface IIocRegistrar
    {
        /// <summary>
        /// 添加新的约定
        /// </summary>
        /// <param name="registrar"></param>
        void AddConventionalRegistrar(IConventionalDependencyRegistrar registrar);
        /// <summary>
        /// 注册所有约定的类型
        /// </summary>
        /// <param name="assembly">要注册的程序集</param>
        void RegisterAssemblyByConvention(Assembly assembly);
        /// <summary>
        /// 注册所有约定的类型
        /// </summary>
        /// <param name="assembly">要注册的程序集</param>
        /// <param name="config">附加配置</param>
        void RegisterAssemblyByConvention(Assembly assembly, ConventionalRegistrationConfig config);
        /// <summary>
        /// 注入指定类型
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <param name="lifeStyle">生命周期</param>
        void Register<T>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
             where T : class;
        /// <summary>
        /// 注入指定类型
        /// </summary>
        /// <param name="type">要注入的类型</param>
        /// <param name="lifeStyle">生命周期</param>
        void Register(Type type, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton);
        /// <summary>
        /// 注入指定类型，显示指定实现
        /// </summary>
        /// <typeparam name="TType">要注入的类型参数</typeparam>
        /// <typeparam name="TImpl">具体实现类的类型参数</typeparam>
        /// <param name="lifeStyle">生命周期</param>
        void Register<TType, TImpl>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
            where TType : class
            where TImpl : class, TType;
        /// <summary>
        /// 注入指定类型，显示指定实现
        /// </summary>
        /// <param name="type">要注入的接口类型</param>
        /// <param name="impl">实现了该接口的类型</param>
        /// <param name="lifeStyle">生命周期</param>
        void Register(Type type, Type impl, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton);
        /// <summary>
        /// 判断类型是否注入到容器当中
        /// </summary>
        /// <param name="type">类型</param>
        bool IsRegistered(Type type);
        /// <summary>
        /// 判断类型是否注入到容器当中
        /// </summary>
        /// <typeparam name="TType">需要判断的类型参数</typeparam>
        bool IsRegistered<TType>();
    }
}