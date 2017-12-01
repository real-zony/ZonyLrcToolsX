using System;

namespace Zony.Lib.Infrastructures.EventBus.Factories.Internals
{

    internal class FactoryUnregistrar : IDisposable
    {
        private readonly IEventBus m_eventBus;
        private readonly Type m_eventType;
        private readonly IEventHandlerFactory m_factory;

        public FactoryUnregistrar(IEventBus eventBus, Type eventType, IEventHandlerFactory factory)
        {
            m_eventBus = eventBus;
            m_eventType = eventType;
            m_factory = factory;
        }

        public void Dispose()
        {
            m_eventBus.Unregister(m_eventType, m_factory);
        }
    }
}
