using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiectMTP.DTOs;
using proiectMTP.Services;

namespace proiectMTP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    private int CurrentProfessorId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionResponse>>> GetAll([FromQuery] int? studentId)
    {
        var sessions = await _sessionService.GetAllAsync(CurrentProfessorId, studentId);
        return Ok(sessions);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<StudentStatsResponse>> GetStats([FromQuery] int studentId)
    {
        var stats = await _sessionService.GetStatsAsync(CurrentProfessorId, studentId);
        return Ok(stats);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SessionResponse>> GetById(int id)
    {
        var session = await _sessionService.GetByIdAsync(CurrentProfessorId, id);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpGet("{id}/file")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var file = await _sessionService.GetFileAsync(CurrentProfessorId, id);
        return file is null ? NotFound() : PhysicalFile(file.AbsolutePath, file.ContentType, file.FileName);
    }

    [HttpPost]
    public async Task<ActionResult<SessionResponse>> Create([FromForm] CreateSessionRequest req)
    {
        var session = await _sessionService.CreateAsync(CurrentProfessorId, req);
        return session is null
            ? BadRequest(new { message = "Student not found or does not belong to you." })
            : CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SessionResponse>> Update(int id, [FromForm] UpdateSessionRequest req)
    {
        var session = await _sessionService.UpdateAsync(CurrentProfessorId, id, req);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _sessionService.DeleteAsync(CurrentProfessorId, id);
        return deleted ? NoContent() : NotFound();
    }
}
