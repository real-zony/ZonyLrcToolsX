using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace Zony.Lib.Infrastructures.EventBus.Factories
{
    /// <summary>
    /// 事件处理器工厂
    /// </summary>
    public interface IEventHandlerFactory
    {
        /// <summary>
        /// 获得一个事件处理器
        /// </summary>
        /// <returns></returns>
        IEventHandler GetHandler();

        /// <summary>
        /// 释放一个事件处理器
        /// </summary>
        /// <param name="handler">要释放的处理器实例</param>
        void ReleaseHandler(IEventHandler handler);
    }
}
