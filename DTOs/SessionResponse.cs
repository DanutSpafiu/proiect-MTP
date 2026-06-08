using proiectMTP.Models;

namespace proiectMTP.DTOs;

public class SessionResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ProfessorId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public bool HasFile { get; set; }

    public static SessionResponse FromSession(Session session) => new()
    {
        Id = session.Id,
        StudentId = session.StudentId,
        ProfessorId = session.ProfessorId,
        Title = session.Title,
        Description = session.Description,
        FileName = session.FileName,
        HasFile = session.StoredPath is not null
    };
}
