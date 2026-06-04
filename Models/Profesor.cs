namespace proiectMTP.Models;

public class Profesor
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
