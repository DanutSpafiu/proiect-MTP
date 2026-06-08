using proiectMTP.Models;

namespace proiectMTP.DTOs;

public class StudentResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public int ProfessorId { get; set; }

    public static StudentResponse FromStudent(Student student) => new()
    {
        Id = student.Id,
        Name = student.Name,
        Email = student.Email,
        Phone = student.Phone,
        Subject = student.Subject,
        ProfessorId = student.ProfessorId
    };
}
