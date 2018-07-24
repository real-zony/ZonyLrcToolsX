using System;

namespace Zony.Lib.Infrastructures.EventBus
{
    public interface IEventData
    {
        /// <summary>
        /// 事件产生时间
        /// </summary>
        DateTime EventTime { get; set; }

        /// <summary>
        /// 触发事件的源对象
        /// </summary>
        object EventSource { get; set; }
    }
}
