namespace proiectMTP.Models;

public enum SessionStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2
}

public class Session
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public string? StoredPath { get; set; }
    public string? ContentType { get; set; }
    public DateTime Date { get; set; }
    public int Duration { get; set; }
    public string? Subject { get; set; }
    public decimal Price { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public string? Notes { get; set; }
    public bool SentReminder { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int ProfessorId { get; set; }
    public Profesor? Professor { get; set; }
}
