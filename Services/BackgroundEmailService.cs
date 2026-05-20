using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;

namespace PayroTech.Services;

public interface IBackgroundEmailService
{
    void QueueEmail(string toEmail, string toName, string subject, string htmlContent);
    int GetQueueCount();
}

public class BackgroundEmailService : IBackgroundEmailService, IHostedService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackgroundEmailService> _logger;
    private readonly ConcurrentQueue<EmailMessage> _emailQueue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private Task? _backgroundTask;
    private readonly CancellationTokenSource _shutdownToken = new();

    public BackgroundEmailService(
        IConfiguration configuration,
        ILogger<BackgroundEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void QueueEmail(string toEmail, string toName, string subject, string htmlContent)
    {
        var email = new EmailMessage
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            HtmlContent = htmlContent,
            QueuedAt = DateTime.UtcNow
        };

        _emailQueue.Enqueue(email);
        _signal.Release();
        _logger.LogInformation("📧 Email queued for {Email}. Queue size: {Count}", toEmail, _emailQueue.Count);
    }

    public int GetQueueCount() => _emailQueue.Count;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 Background Email Service started");
        _backgroundTask = Task.Run(() => ProcessEmailQueueAsync(_shutdownToken.Token), cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Background Email Service stopping...");
        _shutdownToken.Cancel();
        
        if (_backgroundTask != null)
        {
            await Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        
        _logger.LogInformation("✅ Background Email Service stopped. Remaining emails in queue: {Count}", _emailQueue.Count);
    }

    private async Task ProcessEmailQueueAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("📬 Email queue processor started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Wait for signal or cancellation
                await _signal.WaitAsync(cancellationToken);

                if (_emailQueue.TryDequeue(out var email))
                {
                    await SendEmailAsync(email, cancellationToken);
                    
                    // Small delay between emails to avoid overwhelming SMTP server
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in email queue processor");
                await Task.Delay(5000, cancellationToken); // Wait before retrying
            }
        }

        _logger.LogInformation("📬 Email queue processor stopped");
    }

    private async Task SendEmailAsync(EmailMessage email, CancellationToken cancellationToken)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["EmailSettings:FromName"] ?? "PayroTech System";
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(smtpServer) || 
                string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("❌ SMTP configuration incomplete. SmtpServer={S}, Username={U}", 
                    smtpServer ?? "null", smtpUsername ?? "null");
                return;
            }

            _logger.LogInformation("📤 Sending email to {Email}: {Subject}", email.ToEmail, email.Subject);

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true, // Always enforce TLS for SMTP
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = email.Subject,
                Body = email.HtmlContent,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
            mailMessage.To.Add(new MailAddress(email.ToEmail, email.ToName));

            await client.SendMailAsync(mailMessage, cancellationToken);

            var elapsed = DateTime.UtcNow - email.QueuedAt;
            _logger.LogInformation("✅ Email sent successfully to {Email} (queued for {Seconds:F1}s)", 
                email.ToEmail, elapsed.TotalSeconds);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "❌ SMTP error sending email to {Email}. Status: {Status}, Message: {Msg}", 
                email.ToEmail, ex.StatusCode, ex.Message);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Email sending to {Email} was cancelled", email.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {Email}. Type: {Type}", 
                email.ToEmail, ex.GetType().Name);
        }
    }

    public void Dispose()
    {
        _shutdownToken?.Cancel();
        _shutdownToken?.Dispose();
        _signal?.Dispose();
    }

    private class EmailMessage
    {
        public string ToEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public DateTime QueuedAt { get; set; }
    }
}
