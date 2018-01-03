using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace ZonyLrcTools.Events
{
    public class CheckUpdateEventData : EventData
    {

    }

    public class CheckUpdateEvent : IEventHandler<CheckUpdateEventData>, ITransientDependency
    {
        public void HandleEvent(CheckUpdateEventData eventData)
        {

        }
    }
}
