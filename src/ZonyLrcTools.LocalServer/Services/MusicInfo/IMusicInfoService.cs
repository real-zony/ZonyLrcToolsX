using ZonyLrcTools.LocalServer.Contract.Dtos;
using ZonyLrcTools.LocalServer.Services.MusicInfo.Dtos;

namespace ZonyLrcTools.LocalServer.Services.MusicInfo;

public interface IMusicInfoService
{
    Task<PagedListResultDto<MusicInfoListItemDto>> GetMusicInfoListAsync(MusicInfoListInput input);
}