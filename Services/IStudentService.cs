using proiectMTP.DTOs;

namespace proiectMTP.Services;

public interface IStudentService
{
    Task<StudentResponse> CreateAsync(int professorId, CreateStudentRequest req);
    Task<IEnumerable<StudentResponse>> GetAllAsync(int professorId);
    Task<StudentResponse?> GetByIdAsync(int professorId, int id);
    Task<StudentResponse?> UpdateAsync(int professorId, int id, UpdateStudentRequest req);
    Task<bool> DeleteAsync(int professorId, int id);
}