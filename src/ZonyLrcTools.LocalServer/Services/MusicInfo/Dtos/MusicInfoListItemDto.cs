using ZonyLrcTools.LocalServer.Contract.Dtos;

namespace ZonyLrcTools.LocalServer.Services.MusicInfo.Dtos;

public class MusicInfoListItemDto
{
    public string Name { get; set; }

    public int Size { get; set; }

    public int Status { get; set; }
}

public class MusicInfoListInput : PagedListRequestDto
{
}