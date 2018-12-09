using System;
using System.IO;
using Zony.Lib.Infrastructures.Common.Interfaces;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common.Extensions;
using ZonyLrcTools.Encoders.Provider;

namespace ZonyLrcTools.Events
{
    public class LyricDownLoadCompleteEventData : EventData
    {
        public byte[] LyricData { get; set; }
        public MusicInfoModel Info { get; set; }
    }

    public class LyricDownLoadCompleteEvent : IEventHandler<LyricDownLoadCompleteEventData>, ITransientDependency
    {
        private readonly IEncoderProvider _encoder;
        private readonly IConfigurationManager _configMgr;

        public LyricDownLoadCompleteEvent(IEncoderProvider encoder, IConfigurationManager configMgr)
        {
            _encoder = encoder;
            _configMgr = configMgr;
        }

        public void HandleEvent(LyricDownLoadCompleteEventData eventData)
        {
            byte[] lyricData = _encoder.GetEncoder(_configMgr.ConfigModel.EncodingName).Encoding(eventData.LyricData);

            string lyricFilePath = SavePath(eventData.Info);

            using (FileStream lyricFile = new FileStream(lyricFilePath, FileMode.OpenOrCreate))
            {
                lyricFile.Write(lyricData, 0, lyricData.Length);
            }

            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[eventData.Info.Index].SubItems[4].Text = AppConsts.Status_Music_Success;
        }

        private string SavePath(MusicInfoModel info)
        {
            if (string.IsNullOrEmpty(info.FilePath))
            {
                string lyricsPath = Environment.CurrentDirectory + "\\Lyrics";
                if (!Directory.Exists(lyricsPath)) Directory.CreateDirectory(lyricsPath);
                return $"{lyricsPath}\\{info.Song.ReplaceBadCharOfFileName()}-{info.Artist.ReplaceBadCharOfFileName()}.lrc";
            }

            string fileName = Path.GetFileNameWithoutExtension(info.FilePath);
            return $"{Path.GetDirectoryName(info.FilePath)}\\{fileName}.lrc";
        }
    }
}