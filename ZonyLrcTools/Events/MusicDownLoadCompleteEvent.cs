using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Models;
using Zony.Lib.Infrastructures.EventBus;

namespace ZonyLrcTools.Events
{
    public class MusicDownLoadCompleteEventData : EventData
    {
        public byte[] LyricData { get; set; }
        public MusicInfoModel Info { get; set; }
    }

    public class MusicDownLoadCompleteEvent : IEventHandler<MusicDownLoadCompleteEventData>, ITransientDependency
    {
        public void HandleEvent(MusicDownLoadCompleteEventData eventData)
        {

        }
    }
}
