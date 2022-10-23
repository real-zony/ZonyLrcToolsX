namespace ZonyLrcTools.LocalServer.Contract.Dtos;

public class PagedListResultDto<T>
{
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}