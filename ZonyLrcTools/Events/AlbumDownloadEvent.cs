using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Enums;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Events
{
    public class AlbumdownloadEventData : EventData
    {
        public IEnumerable<MusicInfoModel> MusicInfos { get; set; }
        public AlbumdownloadEventData(IEnumerable<MusicInfoModel> musicInfos)
        {
            MusicInfos = musicInfos;
        }
    }

    public class AlbumDownloadEvent : IEventHandler<AlbumdownloadEventData>, ITransientDependency
    {
        private readonly IPluginManager _pluginManager;
        private readonly IConfigurationManager _configMgr;

        public AlbumDownloadEvent(IPluginManager pluginManager, IConfigurationManager configMgr)
        {
            _pluginManager = pluginManager;
            _configMgr = configMgr;
        }

        public async void HandleEvent(AlbumdownloadEventData eventData)
        {
            if (GlobalContext.Instance.MusicInfos.Count == 0 || GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items.Count == 0) MessageBox.Show("你还没有添加歌曲文件!", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var albumPlugin = _pluginManager.GetPlugin<IPluginAlbumDownloader>(_configMgr.ConfigModel.PluginOptions);
            var tagPlugin = _pluginManager.GetPlugin<IPluginAcquireMusicInfo>(_configMgr.ConfigModel.PluginOptions);

            await Task.Run(() =>
            {
                Parallel.ForEach(eventData.MusicInfos, new ParallelOptions { MaxDegreeOfParallelism = _configMgr.ConfigModel.DownloadThreadNumber }, info =>
                  {
                      try
                      {
                          if (!albumPlugin.DownlaodAblumImage(info, out byte[] imgData))
                          {
                              GlobalContext.Instance.SetItemStatus(info.Index, AppConsts.Status_Music_Failed);
                              return;
                          }

                          if (!tagPlugin.SaveAlbumImage(info.FilePath, imgData))
                          {
                              GlobalContext.Instance.SetItemStatus(info.Index, AppConsts.Status_Music_Failed);
                              return;
                          }

                          GlobalContext.Instance.SetItemStatus(info.Index, AppConsts.Status_Music_Success);
                      }
                      catch (ServiceUnavailableException)
                      {
                          info.Status = MusicInfoEnum.Unavailble;
                          GlobalContext.Instance.SetItemStatus(info.Index, AppConsts.Status_Music_Unavailablel);
                      }
                      finally
                      {
                          // 进度条自增
                          GlobalContext.Instance.SetBottomStatusText($"{AppConsts.Status_Bottom_DownLoadHead}{info.Song}");
                          GlobalContext.Instance.UIContext.Bottom_ProgressBar.Value++;
                      }
                  });
            });
        }
    }
}
