using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace proiectMTP.Services;

public class EmailJsEmailSender : IEmailSender
{
    private const string ApiUrl = "https://api.emailjs.com/api/v1.0/email/send";

    private readonly HttpClient _http;
    private readonly EmailJsSettings _settings;

    public EmailJsEmailSender(HttpClient http, IOptions<EmailJsSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        var payload = new
        {
            service_id = _settings.ServiceId,
            template_id = _settings.TemplateId,
            user_id = _settings.PublicKey,
            accessToken = _settings.PrivateKey,
            template_params = new
            {
                to_email = toEmail,
                subject,
                message = body
            }
        };

        using var response = await _http.PostAsJsonAsync(ApiUrl, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"EmailJS send failed ({(int)response.StatusCode}): {error}");
        }
    }
}
