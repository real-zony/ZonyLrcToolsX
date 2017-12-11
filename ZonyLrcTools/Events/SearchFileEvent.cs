using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Interfaces;
using ZonyLrcTools.Events.UIEvents;

namespace ZonyLrcTools.Events
{
    public class SearchFileEventData : EventData
    {
    }

    public class SearchFileEvent : IEventHandler<SearchFileEventData>, ITransientDependency
    {
        private readonly IFileSearchProvider m_searchProvider;
        private readonly IConfigurationManager m_settingManager;

        public SearchFileEvent(IFileSearchProvider searchProvider, IConfigurationManager settingManager)
        {
            m_searchProvider = searchProvider;
            m_settingManager = settingManager;
        }

        public async void HandleEvent(SearchFileEventData eventData)
        {
            FolderBrowserDialog _dlg = new FolderBrowserDialog()
            {
                Description = "请选择歌曲所在目录.",
            };

            if (_dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(_dlg.SelectedPath))
            {
                EventBus.Default.Trigger<UIComponentDisableEventData>();

                var _files = await m_searchProvider.FindFilesAsync(_dlg.SelectedPath, new string[] { "*.mp3", "*.flac", "*.ape", "*.m4a" });

                if (_files.Count == 0) MessageBox.Show("没有找到任何文件。", AppConsts.Msg_Information, MessageBoxButtons.OK, MessageBoxIcon.Information);

                MessageBox.Show(BuildSuccessMsg(_files), "搜索完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                EventBus.Default.Trigger<UIComponentEnableEventData>();
                EventBus.Default.Trigger(new MusicInfoLoadEventData()
                {
                    MusicFilePaths = _files
                });
            }
        }

        private string BuildSuccessMsg(Dictionary<string, List<string>> files)
        {
            StringBuilder _builder = new StringBuilder();
            _builder.Append($"文件查找完成，共搜索到音乐文件 {files.Sum(x => x.Value.Count)} 个。\n");
            _builder.Append($"--------------------\n");

            foreach (var item in files)
            {
                _builder.Append($"{item.Key}:{item.Value.Count} 个\n");
            }

            return _builder.ToString();
        }
    }
}
