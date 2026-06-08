using Microsoft.EntityFrameworkCore;
using proiectMTP.Data;
using proiectMTP.Models;

namespace proiectMTP.Services;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public ReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReminderBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var minutes = configuration.GetValue<int?>("Reminders:CheckIntervalMinutes") ?? 15;
        _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder service started, checking every {Minutes} minute(s).", _interval.TotalMinutes);

        using var timer = new PeriodicTimer(_interval);
        do
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process session reminders.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var now = DateTime.UtcNow;
        var windowEnd = now.AddHours(24);

        var dueSessions = await db.Sessions
            .Include(s => s.Student)
            .Include(s => s.Professor)
            .Where(s => s.Status == SessionStatus.Scheduled
                        && !s.SentReminder
                        && s.Date > now
                        && s.Date <= windowEnd)
            .ToListAsync(ct);

        if (dueSessions.Count == 0)
            return;

        _logger.LogInformation("Found {Count} session(s) due for a reminder.", dueSessions.Count);

        foreach (var session in dueSessions)
        {
            if (string.IsNullOrEmpty(session.Professor?.Email))
            {
                _logger.LogWarning("Session {SessionId} has no professor email; skipping reminder.", session.Id);
                continue;
            }

            var studentName = session.Student?.Name ?? "your student";
            var subject = $"Reminder: upcoming session \"{session.Title}\" in less than 24 hours";
            var body =
                $"Hello {session.Professor.Name},\n\n" +
                $"This is a reminder that you have an upcoming session \"{session.Title}\" " +
                $"with {studentName}, scheduled for {session.Date:dddd, dd MMMM yyyy HH:mm} (UTC).\n\n" +
                (string.IsNullOrEmpty(session.Description) ? "" : $"Details: {session.Description}\n\n") +
                "See you there!";

            try
            {
                await email.SendAsync(session.Professor.Email, subject, body, ct);
                session.SentReminder = true;
                _logger.LogInformation("Reminder sent for session {SessionId} to {Email}.", session.Id, session.Professor.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for session {SessionId}; will retry next cycle.", session.Id);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
