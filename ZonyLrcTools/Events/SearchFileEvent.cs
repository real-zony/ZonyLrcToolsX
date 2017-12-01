using System.Collections.Generic;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class ISearchFileEventData : EventData
    {

    }

    public class SearchFileEvent : IEventHandler<ISearchFileEventData>
    {
        private readonly ISearchProvider m_searchProvider;
        public SearchFileEvent(ISearchProvider searchProvider)
        {
            m_searchProvider = searchProvider;
        }

        public void HandleEvent(ISearchFileEventData eventData)
        {
            FolderBrowserDialog _dlg = new FolderBrowserDialog()
            {
                Description = "请选择歌曲所在目录.",
            };

            if (_dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(_dlg.SelectedPath))
            {
                List<string> _files = m_searchProvider.FindFiles(_dlg.SelectedPath);
            }
        }
    }
}