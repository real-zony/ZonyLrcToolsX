namespace Zony.Lib.Infrastructures.EventBus.Handlers
{
    /// <summary>
    /// 事件处理程序接口
    /// </summary>
    /// <typeparam name="TEventData">事件数据</typeparam>
    public interface IEventHandler<in TEventData> : IEventHandler
    {
        /// <summary>
        /// 处理事件的方法
        /// </summary>
        /// <param name="eventData">传递过来的事件数据</param>
        void HandleEvent(TEventData eventData);
    }
}
