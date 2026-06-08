using System.Globalization;
using Microsoft.EntityFrameworkCore;
using proiectMTP.Data;
using proiectMTP.DTOs;
using proiectMTP.Models;

namespace proiectMTP.Services;

public class SessionService : ISessionService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public SessionService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<SessionResponse?> CreateAsync(int professorId, CreateSessionRequest req)
    {
        var studentOwned = await _db.Students
            .AnyAsync(s => s.Id == req.StudentId && s.ProfessorId == professorId);

        if (!studentOwned)
            return null;

        var session = new Session
        {
            Title = req.Title,
            Description = req.Description,
            Date = req.Date,
            StudentId = req.StudentId,
            ProfessorId = professorId
        };

        if (req.File is not null)
            await AttachFileAsync(session, req.File);

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return SessionResponse.FromSession(session);
    }

    public async Task<IEnumerable<SessionResponse>> GetAllAsync(int professorId, int? studentId)
    {
        var query = _db.Sessions.Where(s => s.ProfessorId == professorId);

        if (studentId is not null)
            query = query.Where(s => s.StudentId == studentId);

        var sessions = await query.ToListAsync();
        return sessions.Select(SessionResponse.FromSession);
    }

    public async Task<SessionResponse?> GetByIdAsync(int professorId, int id)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ProfessorId == professorId && s.Id == id);

        return session is null ? null : SessionResponse.FromSession(session);
    }

    public async Task<SessionResponse?> UpdateAsync(int professorId, int id, UpdateSessionRequest req)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ProfessorId == professorId && s.Id == id);

        if (session is null)
            return null;

        session.Title = req.Title;
        session.Description = req.Description;

        if (session.Date != req.Date)
        {
            session.Date = req.Date;
            session.SentReminder = false;
        }

        if (req.File is not null)
        {
            DeleteStoredFile(session.StoredPath);
            await AttachFileAsync(session, req.File);
        }
        else if (req.RemoveFile)
        {
            DeleteStoredFile(session.StoredPath);
            session.FileName = null;
            session.StoredPath = null;
            session.ContentType = null;
        }

        await _db.SaveChangesAsync();
        return SessionResponse.FromSession(session);
    }

    public async Task<bool> DeleteAsync(int professorId, int id)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ProfessorId == professorId && s.Id == id);

        if (session is null)
            return false;

        DeleteStoredFile(session.StoredPath);
        _db.Sessions.Remove(session);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<SessionFileResult?> GetFileAsync(int professorId, int id)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ProfessorId == professorId && s.Id == id);

        if (session?.StoredPath is null)
            return null;

        var absolutePath = Path.Combine(_env.ContentRootPath, session.StoredPath);
        if (!File.Exists(absolutePath))
            return null;

        return new SessionFileResult(
            absolutePath,
            session.ContentType ?? "application/octet-stream",
            session.FileName ?? "download");
    }

    public async Task<StudentStatsResponse> GetStatsAsync(int professorId, int studentId)
    {
        var sessions = await _db.Sessions
            .Where(s => s.ProfessorId == professorId && s.StudentId == studentId)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var ordered = sessions
            .Select(s => DateTime.SpecifyKind(s.Date, DateTimeKind.Utc))
            .OrderBy(d => d)
            .ToList();

        var past = ordered.Where(d => d <= now).ToList();
        var upcoming = ordered.Where(d => d > now).ToList();
        var withFile = sessions.Count(s => s.StoredPath is not null);

        DateTime? first = ordered.Count > 0 ? ordered[0] : null;
        DateTime? lastPast = past.Count > 0 ? past[^1] : null;
        DateTime? nextUp = upcoming.Count > 0 ? upcoming[0] : null;
        int? daysSinceLast = lastPast is not null
            ? (int)Math.Floor((now - lastPast.Value).TotalDays)
            : null;

        var months = new List<MonthlyCount>();
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        for (var i = 5; i >= 0; i--)
        {
            var m = monthStart.AddMonths(-i);
            months.Add(new MonthlyCount
            {
                Year = m.Year,
                Month = m.Month - 1,
                Label = m.ToString("MMM", CultureInfo.InvariantCulture),
                Count = 0
            });
        }

        foreach (var d in ordered)
        {
            var bucket = months.FirstOrDefault(b => b.Year == d.Year && b.Month == d.Month - 1);
            if (bucket is not null)
                bucket.Count++;
        }

        return new StudentStatsResponse
        {
            Total = sessions.Count,
            UpcomingCount = upcoming.Count,
            WithFile = withFile,
            WithFilePercent = sessions.Count == 0
                ? 0
                : (int)Math.Round((double)withFile / sessions.Count * 100),
            FirstSession = first,
            LastSession = lastPast,
            NextSession = nextUp,
            DaysSinceLast = daysSinceLast,
            Months = months
        };
    }

    private async Task AttachFileAsync(Session session, IFormFile file)
    {
        var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "sessions");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadsDir, storedName);

        await using (var stream = File.Create(absolutePath))
        {
            await file.CopyToAsync(stream);
        }

        session.FileName = Path.GetFileName(file.FileName);
        session.StoredPath = Path.Combine("uploads", "sessions", storedName);
        session.ContentType = file.ContentType;
    }

    private void DeleteStoredFile(string? storedPath)
    {
        if (string.IsNullOrEmpty(storedPath))
            return;

        var absolutePath = Path.Combine(_env.ContentRootPath, storedPath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }
}
