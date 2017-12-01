using System;
using Zony.Lib.Infrastructures.Dependency;

namespace Zony.Lib.Infrastructures.EventBus.Handlers.Internals
{
    internal class ActionEventHandler<TEventData> : IEventHandler<TEventData>, ITransientDependency
    {
        public Action<TEventData> Action { get; private set; }

        public ActionEventHandler(Action<TEventData> handler) => Action = handler;

        public void HandleEvent(TEventData eventData)
        {
            Action(eventData);
        }
    }
}
