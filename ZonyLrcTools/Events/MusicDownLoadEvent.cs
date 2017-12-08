using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace ZonyLrcTools.Events
{
    public class MusicDownLoadEventData : EventData
    {

    }

    public class MusicDownLoadEvent : IEventHandler<MusicDownLoadEvent>,ITransientDependency
    {
        public void HandleEvent(MusicDownLoadEvent eventData)
        {

        }
    }
}
