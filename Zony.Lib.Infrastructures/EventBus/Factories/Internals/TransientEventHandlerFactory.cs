using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace Zony.Lib.Infrastructures.EventBus.Factories.Internals
{
    internal class TransientEventHandlerFactory<THandler> : IEventHandlerFactory where THandler : IEventHandler, new()
    {
        public IEventHandler GetHandler()
        {
            return new THandler();
        }

        public void ReleaseHandler(IEventHandler handler)
        {
        }
    }
}
