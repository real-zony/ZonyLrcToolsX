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
using ZonyLrcTools.Encoders;

namespace ZonyLrcTools.Events
{
    public class LyricDownLoadCompleteEventData : EventData
    {
        public byte[] LyricData { get; set; }
        public MusicInfoModel Info { get; set; }
    }

    public class LyricDownLoadCompleteEvent : IEventHandler<LyricDownLoadCompleteEventData>, ITransientDependency
    {
        private readonly IEncoderProvider m_encoder;
        private readonly IConfigurationManager m_configMgr;

        public LyricDownLoadCompleteEvent(IEncoderProvider encoder, IConfigurationManager configMgr)
        {
            m_encoder = encoder;
            m_configMgr = configMgr;
        }

        public void HandleEvent(LyricDownLoadCompleteEventData eventData)
        {
            byte[] _lyricData = m_encoder.GetEncoder(m_configMgr.ConfigModel.EncodingName).Encoding(eventData.LyricData);

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