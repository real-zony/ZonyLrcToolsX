using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using ZonyLrcTools.Common.Interfaces;
using Zony.Lib.Plugin.Common;

namespace ZonyLrcTools.Events
{
    public class ProgramExitEventData : EventData
    {

    }

    public class ProgramExitEvent : IEventHandler<ProgramExitEventData>,ITransientDependency
    {
        private readonly IConfigurationManager m_configMgr;

        public ProgramExitEvent(IConfigurationManager configMgr)
        {
            m_configMgr = configMgr;
        }

        public void HandleEvent(ProgramExitEventData eventData)
        {
            m_configMgr.SaveConfiguration();
            GlobalContext.Instance.LyricDownloadState = false;
        }
    }
}
