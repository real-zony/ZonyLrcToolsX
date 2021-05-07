using System;
using System.Threading.Tasks;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    public class TaglibTagInfoProvider : ITagInfoProvider, ITransientDependency
    {
        public int Priority => 1;

        public string Name => ConstantName;
        public const string ConstantName = "Taglib";

        public async ValueTask<MusicInfo> LoadAsync(string filePath)
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

                if (songName == null && songArtist == null)
                {
                    return null;
                }

                return new MusicInfo(filePath, songName, songArtist);
            }
            catch (Exception ex)
            {
                throw new ErrorCodeException(ErrorCodes.TagInfoProviderLoadInfoFailed, ex.Message, filePath);
            }
        }
    }
}