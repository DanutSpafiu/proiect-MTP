using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiectMTP.DTOs;
using proiectMTP.Services;

namespace proiectMTP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    private int CurrentProfessorId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentResponse>>> GetAll()
    {
        var students = await _studentService.GetAllAsync(CurrentProfessorId);
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentResponse>> GetById(int id)
    {
        var student = await _studentService.GetByIdAsync(CurrentProfessorId, id);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Create(CreateStudentRequest req)
    {
        var student = await _studentService.CreateAsync(CurrentProfessorId, req);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StudentResponse>> Update(int id, UpdateStudentRequest req)
    {
        var student = await _studentService.UpdateAsync(CurrentProfessorId, id, req);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var deleted = await _studentService.DeleteAsync(CurrentProfessorId, id);
        return deleted ? NoContent() : NotFound();
    }
}
