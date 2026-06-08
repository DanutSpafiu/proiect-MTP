using System.ComponentModel.DataAnnotations;

namespace proiectMTP.DTOs;

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);
