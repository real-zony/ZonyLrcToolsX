using System;
using System.Threading.Tasks;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    /// <summary>
    /// 基于 TagLib 的标签信息解析器。
    /// </summary>
    public class TaglibTagInfoProvider : ITagInfoProvider, ISingletonDependency
    {
        public string Name => ConstantName;
        public const string ConstantName = "Taglib";

        public async ValueTask<MusicInfo?> LoadAsync(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);

                var songName = file.Tag.Title;
                var songArtist = file.Tag.FirstPerformer;

                if (!string.IsNullOrEmpty(file.Tag.FirstAlbumArtist))
                {
                    songArtist = file.Tag.FirstAlbumArtist;
                }

                await ValueTask.CompletedTask;

                return songName == null ? null : new MusicInfo(filePath, songName, songArtist);
            }
            catch (Exception ex)
            {
                throw new ErrorCodeException(ErrorCodes.TagInfoProviderLoadInfoFailed, ex.Message, filePath);
            }
        }
    }
}