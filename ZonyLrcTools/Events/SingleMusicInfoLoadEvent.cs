using System.Drawing;
using System.IO;
using System.Linq;
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
    public class SingleMusicInfoLoadEventData : EventData
    {

    }

    public class SingleMusicInfoLoadEvent : IEventHandler<SingleMusicInfoLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_plugManager;

        public SingleMusicInfoLoadEvent(IPluginManager plugManager)
        {
            m_plugManager = plugManager;
        }

        public void HandleEvent(SingleMusicInfoLoadEventData eventData)
        {
            ListViewNF _listView = GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList;

            if (_listView.SelectedItems == null) return;
            int _selectIndex = _listView.SelectedItems[0].Index;

            IPluginAcquireMusicInfo _acquire = m_plugManager.GetPlugin<IPluginAcquireMusicInfo>();
            MusicInfoModel _info = GlobalContext.Instance.MusicInfos[_selectIndex];
            Stream _imgStream = _acquire.LoadAlbumImage(_info.FilePath);

            // 加载信息
            if (_imgStream != null)
            {
                GlobalContext.Instance.UIContext.Right_PictureBox_AlbumImage.Image = Image.FromStream(_imgStream);
            }
            GlobalContext.Instance.UIContext.Right_TextBox_MusicTitle.Text = _info.Song;
            GlobalContext.Instance.UIContext.Right_TextBox_MusicArtist.Text = _info.Artist;
            GlobalContext.Instance.UIContext.Right_TextBox_MusicAblum.Text = _info.Album;
            GlobalContext.Instance.UIContext.Right_TextBox_MusicBuildInLyric.Text = _info.BuildInLyric;
        }
    }
}