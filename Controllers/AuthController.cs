using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiectMTP.Data;
using proiectMTP.DTOs;
using proiectMTP.Models;
using proiectMTP.Services;

namespace proiectMTP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Professors.AnyAsync(p => p.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var professor = new Profesor
        {
            Name = request.Name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Professors.Add(professor);
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _tokenService.CreateToken(professor);
        return Ok(new AuthResponse(professor.Id, professor.Name, professor.Email, token, expiresAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var professor = await _db.Professors.FirstOrDefaultAsync(p => p.Email == email);
        if (professor is null || !BCrypt.Net.BCrypt.Verify(request.Password, professor.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var (token, expiresAt) = _tokenService.CreateToken(professor);
        return Ok(new AuthResponse(professor.Id, professor.Name, professor.Email, token, expiresAt));
    }
}
