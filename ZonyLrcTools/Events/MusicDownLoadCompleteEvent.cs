using System;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Models;

namespace ZonyLrcTools.Events
{
    public class MusicDownLoadCompleteEventData : MainUIComponentContext
    {

    }

    public class MusicDownLoadCompleteEvent : IEventHandler<MusicDownLoadCompleteEventData>, ITransientDependency
    {
        public void HandleEvent(MusicDownLoadCompleteEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}
