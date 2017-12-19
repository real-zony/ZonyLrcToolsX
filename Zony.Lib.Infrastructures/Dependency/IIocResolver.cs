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
        /// <summary>
        /// 解析所有对象
        /// </summary>
        /// <typeparam name="T">要解析的接口类型</typeparam>
        T[] ResolveAll<T>();
        /// <summary>
        /// 解析所有对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentsAsAnonymousType"></param>
        /// <returns></returns>
        T[] ResolveAll<T>(object argumentsAsAnonymousType);
        object[] ResolveAll(Type type);
        object[] ResolveAll(Type type, object argumentsAsAnonymousType);
        /// <summary>
        /// 从依赖容器当中释放对象
        /// </summary>
        /// <param name="obj">需要释放的对象实例</param>
        void Release(object obj);
        /// <summary>
        /// 检测某种类型是否被注入到了依赖容器当中
        /// </summary>
        /// <param name="type">需要检测的类型</param>
        bool IsRegistered(Type type);
        /// <summary>
        /// 检测某种类型是否被注入到了依赖容器当中
        /// </summary>
        /// <typeparam name="T">需要检测的类型</typeparam>
        bool IsRegistered<T>();
    }
}