namespace proiectMTP.Models;

public class Student
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public int ProfessorId { get; set; }
    public Profesor? Professor { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
