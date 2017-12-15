using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class MusicInfoLoadEventData : EventData
    {
        public Dictionary<string, List<string>> MusicFilePaths { get; set; }
    }

    public class MusicInfoLoadEvent : IEventHandler<MusicInfoLoadEventData>, ITransientDependency
    {
        public IPluginManager PluginManager { get; set; }

        public async void HandleEvent(MusicInfoLoadEventData eventData)
        {
            List<MusicInfoModel> _infos = PluginManager.GetPlugin<IPluginAcquireMusicInfo>().GetMusicInfos(eventData.MusicFilePaths).OrderBy(z => z.Index).ToList();

            FillGlobalContextMusicInfos(_infos);

            await Task.Run(() =>
            {

                foreach (var _info in GlobalContext.Instance.MusicInfos.OrderBy(z=>z.Index))
                {
                    GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items.Insert(_info.Index, new ListViewItem(new string[]
                    {
                        _info.Song,
                        _info.Artist,
                        _info.Album,
                        _info.TagType,
                        AppConsts.Status_Music_Waiting
                    }));
                }
            });
        }

        private void FillGlobalContextMusicInfos(List<MusicInfoModel> list)
        {
            if (GlobalContext.Instance.MusicInfos == null)
            {
                GlobalContext.Instance.MusicInfos = new List<MusicInfoModel>();
            }

            foreach (var item in list)
            {
                GlobalContext.Instance.MusicInfos.Add(item);
            }
        }
    }
}