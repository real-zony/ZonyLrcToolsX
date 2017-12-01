using System;

namespace Zony.Lib.Infrastructures.EventBus
{
    [Serializable]
    public class EventData : IEventData
    {
        public DateTime EventTime { get; set; }
        public object EventSource { get; set; }

        protected EventData()
        {
            EventTime = DateTime.Now;
        }
    }
}
