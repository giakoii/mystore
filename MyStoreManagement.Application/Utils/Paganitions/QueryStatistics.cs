namespace MyStoreManagement.Application.Utils.Paganitions;

public class QueryStatistics
{
    public long TotalResults { get; set; }
    public TimeSpan Duration { get; set; }
    public string QueryPlan { get; set; } = string.Empty;
}