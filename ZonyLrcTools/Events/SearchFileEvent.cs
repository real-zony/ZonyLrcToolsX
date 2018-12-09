using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Common.Interfaces;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using ZonyLrcTools.Common.Interfaces;
using ZonyLrcTools.Events.UIEvents;

namespace ZonyLrcTools.Events
{
    public class SearchFileEventData : EventData
    {
        /// <summary>
        /// 文件搜索事件
        /// </summary>
        /// <param name="folderPath">是否追加插入文件信息</param>
        /// <param name="isAppendData">待搜索的文件夹路径</param>
        public SearchFileEventData(string folderPath, bool isAppendData = false)
        {
            IsAppendData = isAppendData;
            FolderPath = folderPath;
        }

        /// <summary>
        /// 是否追加插入文件信息
        /// </summary>
        public bool IsAppendData { get; }

        /// <summary>
        /// 待搜索的文件夹路径
        /// </summary>
        public string FolderPath { get; set; }
    }

    public class SearchFileEvent : IEventHandler<SearchFileEventData>, ITransientDependency
    {
        private readonly IFileSearchProvider _searchProvider;
        private readonly IConfigurationManager _settingManager;

        public SearchFileEvent(IFileSearchProvider searchProvider, IConfigurationManager settingManager)
        {
            _searchProvider = searchProvider;
            _settingManager = settingManager;
        }

        public async void HandleEvent(SearchFileEventData eventData)
        {
            EventBus.Default.Trigger<UIComponentDisableEventData>();
            if (!eventData.IsAppendData) EventBus.Default.Trigger<UIClearMusicInfosEventData>();

            GlobalContext.Instance.SetBottomStatusText(AppConsts.Status_Bottom_SearchFilesing);

            var files = await _searchProvider.FindFilesAsync(eventData.FolderPath, _settingManager.ConfigModel.ExtensionsName);
            if (files.Count == 0) MessageBox.Show("没有找到任何文件。", AppConsts.Msg_Information, MessageBoxButtons.OK, MessageBoxIcon.Information);

            MessageBox.Show(BuildCompleteMsg(files), "搜索完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

            EventBus.Default.Trigger(new MusicInfoLoadEventData
            {
                MusicFilePaths = files
            });
        }

        /// <summary>
        /// 构建完成消息
        /// </summary>
        /// <param name="files">搜索出来的文件路径信息字典</param>
        /// <returns></returns>
        private string BuildCompleteMsg(Dictionary<string, List<string>> files)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"文件查找完成，共搜索到音乐文件 {files.Sum(x => x.Value.Count)} 个。\n");
            builder.Append($"--------------------\n");

            foreach (var item in files)
            {
                builder.Append($"{item.Key}:{item.Value.Count} 个\n");
            }

            return builder.ToString();
        }
    }
}
