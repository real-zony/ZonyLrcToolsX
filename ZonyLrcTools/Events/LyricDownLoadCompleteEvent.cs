using System;
using System.IO;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Extensions;
using ZonyLrcTools.Common.Interfaces;

namespace ZonyLrcTools.Events
{
    public class LyricDownLoadCompleteEventData : EventData
    {
        public byte[] LyricData { get; set; }
        public MusicInfoModel Info { get; set; }
    }

    public class LyricDownLoadCompleteEvent : IEventHandler<LyricDownLoadCompleteEventData>, ITransientDependency
    {
        private readonly IEncodingLyricProvider m_encoder;

        public LyricDownLoadCompleteEvent(IEncodingLyricProvider encoder)
        {
            m_encoder = encoder;
        }

        public void HandleEvent(LyricDownLoadCompleteEventData eventData)
        {
            byte[] _lyricData = m_encoder.EncodeText(eventData.LyricData);

            string _lyricFilePath = SavePath(eventData.Info);

            using (FileStream _lyricFile = new FileStream(_lyricFilePath, FileMode.OpenOrCreate))
            {
                _lyricFile.Write(_lyricData, 0, _lyricData.Length);
            }

            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[eventData.Info.Index].SubItems[4].Text = AppConsts.Status_Music_Success;
        }

        private string SavePath(MusicInfoModel info)
        {
            if (string.IsNullOrEmpty(info.FilePath))
            {
                string _lyricsPath = Environment.CurrentDirectory + "\\Lyrics";
                if (!Directory.Exists(_lyricsPath)) Directory.CreateDirectory(_lyricsPath);
                return $"{_lyricsPath}\\{info.Song.ReplaceBadCharOfFileName()}-{info.Artist.ReplaceBadCharOfFileName()}.lrc";
            }
            else
            {
                string _fileName = Path.GetFileNameWithoutExtension(info.FilePath);
                return $"{Path.GetDirectoryName(info.FilePath)}\\{_fileName}.lrc";
            }
        }
    }
}