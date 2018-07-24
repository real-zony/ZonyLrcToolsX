using System.Drawing;
using System.IO;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using Zony.Lib.UIComponents;

namespace ZonyLrcTools.Events
{
    public class SingleMusicInfoLoadEventData : EventData
    {

    }

    public class SingleMusicInfoLoadEvent : IEventHandler<SingleMusicInfoLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager _plugManager;

        public SingleMusicInfoLoadEvent(IPluginManager plugManager)
        {
            _plugManager = plugManager;
        }

        public void HandleEvent(SingleMusicInfoLoadEventData eventData)
        {
            ListViewNF listView = GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList;

            int selectIndex = listView.SelectedItems[0].Index;

            IPluginAcquireMusicInfo acquire = _plugManager.GetPlugin<IPluginAcquireMusicInfo>();
            MusicInfoModel info = GlobalContext.Instance.MusicInfos[selectIndex];
            Stream imgStream = acquire.LoadAlbumImage(info.FilePath);

            // 填充歌曲信息到 UI 
            if (imgStream != null)
            {
                GlobalContext.Instance.UIContext.Right_PictureBox_AlbumImage.Image = Image.FromStream(imgStream);
            }
            GlobalContext.Instance.UIContext.Right_TextBox_MusicTitle.Text = info.Song;
            GlobalContext.Instance.UIContext.Right_TextBox_MusicArtist.Text = info.Artist;
            GlobalContext.Instance.UIContext.Right_TextBox_MusicAblum.Text = info.Album;
            GlobalContext.Instance.UIContext.Right_TextBox_MusicBuildInLyric.Text = info.BuildInLyric;
        }
    }
}