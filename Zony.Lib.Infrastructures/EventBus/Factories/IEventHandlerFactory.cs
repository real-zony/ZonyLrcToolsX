using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace Zony.Lib.Infrastructures.EventBus.Factories
{
    public interface IEventHandlerFactory
    {
        IEventHandler GetHandler();

        void ReleaseHandler(IEventHandler handler);
    }
}
