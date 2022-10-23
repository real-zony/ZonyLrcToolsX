using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.LocalServer.Contract.Dtos;
using ZonyLrcTools.LocalServer.Services.MusicInfo.Dtos;

namespace ZonyLrcTools.LocalServer.Services.MusicInfo;

public class MusicInfoService : ITransientDependency, IMusicInfoService
{
    public async Task<PagedListResultDto<MusicInfoListItemDto>> GetMusicInfoListAsync(MusicInfoListInput input)
    {
        await Task.CompletedTask;

        return new PagedListResultDto<MusicInfoListItemDto>
        {
            Items = new List<MusicInfoListItemDto>
            {
                new MusicInfoListItemDto
                {
                    Name = "测试歌曲",
                    Size = 1024,
                    Status = 1
                },
                new MusicInfoListItemDto
                {
                    Name = "测试歌曲2",
                    Size = 1024,
                    Status = 1
                },
                new MusicInfoListItemDto
                {
                    Name = "测试歌曲3",
                    Size = 1024,
                    Status = 1
                },
            }
        };
    }
}