namespace proiectMTP.DTOs;

public record AuthResponse(
    int Id,
    string Name,
    string Email,
    string Token,
    DateTime ExpiresAt
);
