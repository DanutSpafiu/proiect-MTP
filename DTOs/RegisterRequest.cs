using System.ComponentModel.DataAnnotations;

namespace proiectMTP.DTOs;

public record RegisterRequest(
    [Required] string Name,
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password
);
