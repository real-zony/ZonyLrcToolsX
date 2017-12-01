namespace Zony.Lib.Infrastructures.EventBus.Handlers
{
    public interface IEventHandler<in TEventData> : IEventHandler
    {
        void HandleEvent(TEventData eventData);
    }
}
