using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using Zony.Lib.UIComponents;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class MusicInfoLoadEventData : EventData
    {
        public Dictionary<string, List<string>> MusicFilePaths { get; set; }
        public ListViewNF MusicListView { get; set; }
    }

    public class MusicInfoLoadEvent : IEventHandler<MusicInfoLoadEventData>, ITransientDependency
    {
        public IPluginManager PluginManager { get; set; }

        public async void HandleEvent(MusicInfoLoadEventData eventData)
        {
            List<MusicInfoModel> _infos = PluginManager.GetPlugin<IPluginAcquireMusicInfo>().GetMusicInfos(eventData.MusicFilePaths);

            await Task.Run(() =>
            {

                for (int _index = 0; _index < _infos.Count; _index++)
                {
                    eventData.MusicListView.Items.Insert(_index, new ListViewItem(new string[]
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
    }
}