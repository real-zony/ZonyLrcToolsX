using System;

namespace Zony.Lib.Infrastructures.Dependency
{
    public interface IIocResolver
    {
        /// <summary>
        /// 从依赖容器当中解析对象
        /// </summary>
        /// <typeparam name="T">要解析的对象类型</typeparam>
        T Resolve<T>();
        /// <summary>
        /// 从依赖容器当中解析对象
        /// </summary>
        /// <typeparam name="T">注入的对象类型</typeparam>
        /// <param name="type">要解析出来的对象类型</param>
        T Resolve<T>(Type type);
        /// <summary>
        /// 从依赖容器当中解析对象
        /// </summary>
        /// <typeparam name="T">要解析出来的对象类型那个</typeparam>
        /// <param name="argumentsAsAnonymousType">参数类型</param>
        T Resolve<T>(object argumentsAsAnonymousType);
        /// <summary>
        /// 从依赖容器当中解析对象
        /// </summary>
        /// <param name="type">要解析的对象类型</param>
        object Resolve(Type type);
        /// <summary>
        /// 从依赖容器当中解析对象
        /// </summary>
        /// <param name="type">要解析的对象类型</param>
        /// <param name="argumentsAsAnonymousType">解析参数</param>
        /// <returns></returns>
        object Resolve(Type type, object argumentsAsAnonymousType);
        T[] ResolveAll<T>();
        T[] ResolveAll<T>(object argumentsAsAnonymousType);
        object[] ResolveAll(Type type);
        object[] ResolveAll(Type type, object argumentsAsAnonymousType);
        void Release(object obj);
        bool IsRegistered(Type type);
        bool IsRegistered<T>();
    }
}
