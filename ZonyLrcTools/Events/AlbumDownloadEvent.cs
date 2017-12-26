using System.Collections.Generic;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;
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
        private readonly IPluginManager m_pluginManager;
        private readonly IConfigurationManager m_configMgr;

        public AlbumDownloadEvent(IPluginManager pluginManager, IConfigurationManager configMgr)
        {
            m_pluginManager = pluginManager;
            m_configMgr = configMgr;
        }

        public async void HandleEvent(AlbumdownloadEventData eventData)
        {
            var _albumPlugin = m_pluginManager.GetPlugin<IPluginAlbumDownloader>();
            var _tagPlugin = m_pluginManager.GetPlugin<IPluginAcquireMusicInfo>();

            await Task.Run(() =>
            {
                Parallel.ForEach(eventData.MusicInfos, new ParallelOptions() { MaxDegreeOfParallelism = m_configMgr.ConfigModel.DownloadThreadNumber }, (_info) =>
                  {
                      if (!_albumPlugin.DownlaodAblumImage(_info, out byte[] _imgData))
                      {
                          GlobalContext.Instance.SetItemStatus(_info.Index, AppConsts.Status_Music_Failed);
                          return;
                      }
                      if (!_tagPlugin.SaveAlbumImage(_info.FilePath, _imgData))
                      {
                          GlobalContext.Instance.SetItemStatus(_info.Index, AppConsts.Status_Music_Failed);
                          return;
                      }

                      GlobalContext.Instance.SetItemStatus(_info.Index, AppConsts.Status_Music_Success);
                  });
            });
        }
    }
}
