using System;
using Zony.Lib.Infrastructures.EventBus.Factories;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using System.Threading.Tasks;

namespace Zony.Lib.Infrastructures.EventBus
{
    public interface IEventBus
    {
        #region > 注册 <
        IDisposable Register<TEventData>(Action<TEventData> action) where TEventData : IEventData;
        IDisposable Register<TEventData>(IEventHandler<TEventData> handler) where TEventData : IEventData;
        IDisposable Register<TEventData, THandler>() where TEventData : IEventData where THandler : IEventHandler<TEventData>, new();
        IDisposable Register(Type eventType, IEventHandler handlers);
        IDisposable Register<TEventData>(IEventHandlerFactory handlerFactory) where TEventData : IEventData;
        IDisposable Register(Type eventType, IEventHandlerFactory handlerFactory);
        #endregion

        #region > 取消注册 <
        void Unregister<TEventData>(Action<TEventData> action) where TEventData : IEventData;
        void Unregister<TEventData>(IEventHandler<TEventData> handler) where TEventData : IEventData;
        void Unregister(Type eventType, IEventHandler handler);
        void Unregister<TEventData>(IEventHandlerFactory factory) where TEventData : IEventData;
        void Unregister(Type eventType, IEventHandlerFactory factory);
        void UnregisterAll<TEventData>() where TEventData : IEventData;
        void UnregisterAll(Type eventType);
        #endregion

        #region > 触发器 <
        void Trigger<TEventData>() where TEventData : IEventData;
        void Trigger<TEventData>(TEventData eventData) where TEventData : IEventData;
        void Trigger<TEventData>(object eventSource, TEventData eventData) where TEventData : IEventData;
        void Trigger(Type eventType, IEventData eventData);
        void Trigger(Type eventType, object eventSource, IEventData eventData);
        Task TriggerAsync<TEventData>() where TEventData : IEventData;
        Task TriggerAsync<TEventData>(TEventData eventData) where TEventData : IEventData;
        Task TriggerAsync<TEventData>(object eventSource, TEventData eventData) where TEventData : IEventData;
        Task TriggerAsync(Type eventType, IEventData eventData);
        Task TriggerAsync(Type eventType, object eventSource, IEventData eventData);
        #endregion
    }
}
