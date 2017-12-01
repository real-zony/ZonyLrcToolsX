using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace Zony.Lib.Infrastructures.EventBus.Factories.Internals
{
    internal class SingleInstanceHandlerFactory : IEventHandlerFactory
    {
        public IEventHandler HandlerInstance { get; private set; }

        public SingleInstanceHandlerFactory(IEventHandler handler)
        {
            HandlerInstance = handler;
        }

        public IEventHandler GetHandler()
        {
            return HandlerInstance;
        }

        public void ReleaseHandler(IEventHandler handler)
        {

        }
    }
}
