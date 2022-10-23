using Microsoft.AspNetCore.Mvc;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.LocalServer.Contract.Dtos;
using ZonyLrcTools.LocalServer.Services.MusicInfo;
using ZonyLrcTools.LocalServer.Services.MusicInfo.Dtos;

namespace ZonyLrcTools.LocalServer.Controllers;

[Route("api/music-infos")]
public class MusicInfoController : Controller, IMusicInfoService, ITransientDependency
{
    private readonly IMusicInfoService _musicInfoService;

    public MusicInfoController(IMusicInfoService musicInfoService)
    {
        _musicInfoService = musicInfoService;
    }

    [HttpGet]
    public Task<PagedListResultDto<MusicInfoListItemDto>> GetMusicInfoListAsync(MusicInfoListInput input)
    {
        return _musicInfoService.GetMusicInfoListAsync(input);
    }
}