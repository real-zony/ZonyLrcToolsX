using System;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace Zony.Lib.Infrastructures.EventBus.Factories
{
    /// <summary>
    /// 基于 Ioc 的事件处理器工厂
    /// </summary>
    public class IocHandlerFactory : IEventHandlerFactory
    {
        public Type HandlerType { get; private set; }
        private readonly IIocResolver _iocResolver;

        public IocHandlerFactory(IIocResolver iocResolver, Type handlerType)
        {
            _iocResolver = iocResolver;
            HandlerType = handlerType;
        }

        public IEventHandler GetHandler()
        {
            return _iocResolver.Resolve(HandlerType) as IEventHandler;
        }

        public void ReleaseHandler(IEventHandler handler)
        {
            _iocResolver.Release(handler);
        }
    }
}
