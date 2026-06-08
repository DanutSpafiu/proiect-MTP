using System.ComponentModel.DataAnnotations;

namespace proiectMTP.DTOs;

public class UpdateSessionRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IFormFile? File { get; set; }

    public bool RemoveFile { get; set; }
}
