using proiectMTP.DTOs;

namespace proiectMTP.Services;

public record SessionFileResult(string AbsolutePath, string ContentType, string FileName);

public interface ISessionService
{
    Task<SessionResponse?> CreateAsync(int professorId, CreateSessionRequest req);
    Task<IEnumerable<SessionResponse>> GetAllAsync(int professorId, int? studentId);
    Task<SessionResponse?> GetByIdAsync(int professorId, int id);
    Task<SessionResponse?> UpdateAsync(int professorId, int id, UpdateSessionRequest req);
    Task<bool> DeleteAsync(int professorId, int id);
    Task<SessionFileResult?> GetFileAsync(int professorId, int id);
}
