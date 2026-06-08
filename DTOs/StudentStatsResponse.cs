namespace proiectMTP.DTOs;

public class MonthlyCount
{
    public int Year { get; set; }
    public int Month { get; set; }
    public required string Label { get; set; }
    public int Count { get; set; }
}

public class StudentStatsResponse
{
    public int Total { get; set; }
    public int UpcomingCount { get; set; }
    public int WithFile { get; set; }
    public int WithFilePercent { get; set; }
    public DateTime? FirstSession { get; set; }
    public DateTime? LastSession { get; set; }
    public DateTime? NextSession { get; set; }
    public int? DaysSinceLast { get; set; }
    public List<MonthlyCount> Months { get; set; } = new();
}
