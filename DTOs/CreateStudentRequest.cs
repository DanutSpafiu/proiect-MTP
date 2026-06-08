using System.ComponentModel.DataAnnotations;

namespace proiectMTP.DTOs;

public record CreateStudentRequest(
    [Required] string Name,
    [Required][EmailAddress] string Email,
    string? Phone,
    string? Subject
);
