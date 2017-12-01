using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.EventBus.Factories;
using Zony.Lib.Infrastructures.EventBus.Factories.Internals;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Infrastructures.EventBus.Handlers.Internals;

namespace Zony.Lib.Infrastructures.EventBus
{
    public class EventBus : IEventBus
    {
        public static EventBus Default { get; } = new EventBus();

        private readonly ConcurrentDictionary<Type, List<IEventHandlerFactory>> _handlerFactories;

        public EventBus()
        {
            _handlerFactories = new ConcurrentDictionary<Type, List<IEventHandlerFactory>>();
        }

        public IDisposable Register<TEventData>(Action<TEventData> action) where TEventData : IEventData
        {
            return Register(typeof(TEventData), new ActionEventHandler<TEventData>(action));
        }

        public IDisposable Register<TEventData>(IEventHandler<TEventData> handler) where TEventData : IEventData
        {
            return Register(typeof(TEventData), handler);
        }

        public IDisposable Register<TEventData, THandler>()
            where TEventData : IEventData
            where THandler : IEventHandler<TEventData>, new()
        {
            return Register(typeof(TEventData), new TransientEventHandlerFactory<THandler>());
        }

        public IDisposable Register(Type eventType, IEventHandler Handlers)
        {
            throw new NotImplementedException();
        }

        public IDisposable Register<TEventData>(IEventHandlerFactory handlerFactory) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public IDisposable Register(Type eventType, IEventHandlerFactory handlerFactory)
        {
            throw new NotImplementedException();
        }

        public void Unregister<TEventData>(Action<TEventData> action) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void Unregister<TEventData>(IEventHandler<TEventData> handler) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void Unregister(Type eventType, IEventHandler handler)
        {
            throw new NotImplementedException();
        }

        public void Unregister<TEventData>(IEventHandlerFactory factory) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void Unregister(Type eventType, IEventHandlerFactory factory)
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll<TEventData>() where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll(Type eventType)
        {
            throw new NotImplementedException();
        }

        public void Trigger<TEventData>(TEventData eventData) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void Trigger<TEventData>(object eventSource, TEventData eventData) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void Trigger(Type eventType, IEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void Trigger(Type eventType, object eventSource, IEventData eventData)
        {
            throw new NotImplementedException();
        }

        public Task TriggerAsync<TEventData>(TEventData eventData) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public Task TriggerAsync<TEventData>(object eventSource, TEventData eventData) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public Task TriggerAsync(Type eventType, IEventData eventData)
        {
            throw new NotImplementedException();
        }

        public Task TriggerAsync(Type eventType, object eventSource, IEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void Trigger<TEventData>() where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public Task TriggerAsync<TEventData>() where TEventData : IEventData
        {
            throw new NotImplementedException();
        }
    }
}
