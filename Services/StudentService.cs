using Microsoft.EntityFrameworkCore;
using proiectMTP.Data;
using proiectMTP.DTOs;
using proiectMTP.Models;

namespace proiectMTP.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext _db;

    public StudentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StudentResponse> CreateAsync(int professorId, CreateStudentRequest req)
    {
        var student = new Student
        {
            Name = req.Name,
            Email = req.Email,
            Phone = req.Phone,
            Subject = req.Subject,
            ProfessorId = professorId
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        return StudentResponse.FromStudent(student);
    }

    public async Task<IEnumerable<StudentResponse>> GetAllAsync(int professorId)
    {
        return await _db.Students
            .Where(s => s.ProfessorId == professorId)
            .Select(s => StudentResponse.FromStudent(s))
            .ToListAsync();
    }

    public Task<StudentResponse?> GetByIdAsync(int professorId, int id)
    {
        return _db.Students
            .Where(s => s.ProfessorId == professorId && s.Id == id)
            .Select(s => StudentResponse.FromStudent(s))
            .FirstOrDefaultAsync();
    }

    public async Task<StudentResponse?> UpdateAsync(int professorId, int id, UpdateStudentRequest req)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.ProfessorId == professorId && s.Id == id);

        if (student is null)
            return null;

        student.Name = req.Name;
        student.Email = req.Email;
        student.Phone = req.Phone;
        student.Subject = req.Subject;

        await _db.SaveChangesAsync();

        return StudentResponse.FromStudent(student);
    }

    public async Task<bool> DeleteAsync(int professorId, int id)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.ProfessorId == professorId && s.Id == id);

        if (student is null)
            return false;

        _db.Students.Remove(student);
        await _db.SaveChangesAsync();
        return true;
    }
}
