using System;

namespace Zony.Lib.Infrastructures.EventBus
{
    public interface IEventData
    {
        DateTime EventTime { get; set; }
        object EventSource { get; set; }
    }
}
