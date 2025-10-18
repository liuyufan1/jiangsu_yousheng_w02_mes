namespace w02_mes.sqlServer;

public class PagedResult<T>
{
    public int PageIndex { get; set; }   // 从 1 开始
    public int PageSize  { get; set; }
    public int Total     { get; set; }   // 符合条件的总记录数
    public int Pages     { get; set; }   // 总页数 = ceil(Total / PageSize)
    public List<T> Records { get; set; } = new();
}