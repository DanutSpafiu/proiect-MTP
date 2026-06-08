using System.ComponentModel.DataAnnotations;

namespace proiectMTP.DTOs;

public class CreateSessionRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public IFormFile? File { get; set; }
}
