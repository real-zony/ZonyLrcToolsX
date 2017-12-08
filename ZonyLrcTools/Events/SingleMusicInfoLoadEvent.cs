using System.Drawing;
using System.IO;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using Zony.Lib.UIComponents;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class SingleMusicInfoLoadEventData : MainUIComponentContext
    {

    }

    public class SingleMusicInfoLoadEvent : IEventHandler<SingleMusicInfoLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_plugManager;
        private readonly GlobalContext m_globalContext;

        public SingleMusicInfoLoadEvent(IPluginManager plugManager, GlobalContext globalContext)
        {
            m_plugManager = plugManager;
            m_globalContext = globalContext;
        }

        public void HandleEvent(SingleMusicInfoLoadEventData eventData)
        {
            ListViewNF _listView = eventData.Center_ListViewNF_MusicList;

            if (_listView.SelectedItems == null) return;
            int _selectIndex = _listView.SelectedItems[0].Index;

            IPluginAcquireMusicInfo _acquire = m_plugManager.GetPlugin<IPluginAcquireMusicInfo>();
            Stream _imgStream = _acquire.LoadAlbumImage(m_globalContext.MusicInfos.ToArray()[_selectIndex].FilePath);
            eventData.Right_PictureBox_AlbumImage.Image = Image.FromStream(_imgStream);
        }
    }
}
