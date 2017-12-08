using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class MusicInfoLoadEventData : MainUIComponentContext
    {
        public Dictionary<string, List<string>> MusicFilePaths { get; set; }
    }

    public class MusicInfoLoadEvent : IEventHandler<MusicInfoLoadEventData>, ITransientDependency
    {
        public IPluginManager PluginManager { get; set; }
        public GlobalContext GlobalContext { get; set; }

        public async void HandleEvent(MusicInfoLoadEventData eventData)
        {
            List<MusicInfoModel> _infos = PluginManager.GetPlugin<IPluginAcquireMusicInfo>().GetMusicInfos(eventData.MusicFilePaths);

            FillGlobalContextMusicInfos(_infos);

            await Task.Run(() =>
            {

                for (int _index = 0; _index < _infos.Count; _index++)
                {
                    eventData.Center_ListViewNF_MusicList.Items.Insert(_index, new ListViewItem(new string[]
                    {
                        _infos[_index].Song,
                        _infos[_index].Artist,
                        _infos[_index].Album,
                        _infos[_index].TagType,
                        AppConsts.Status_Music_Waiting
                    }));
                }
            });
        }

        private void FillGlobalContextMusicInfos(List<MusicInfoModel> list)
        {
            GlobalContext.MusicInfos = new ConcurrentBag<MusicInfoModel>();

            foreach (var item in list)
            {
                GlobalContext.MusicInfos.Add(item);
            }
        }
    }
}