using System.Net;
using System.Net.Mail;

namespace PayroTech.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlContent);
    void QueueEmailAsync(string toEmail, string toName, string subject, string htmlContent);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string toName, string companyName, string tempPassword);
    void QueueWelcomeEmailAsync(string toEmail, string toName, string companyName, string tempPassword);
    Task<bool> SendStaffWelcomeEmailAsync(string toEmail, string toName, string companyName, string role, string tempPassword);
    Task<bool> SendPayslipEmailAsync(string toEmail, string toName, string payslipUrl, string period);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);
    Task<bool> SendPayrollApprovalNotificationAsync(string toEmail, string toName, string teamName, decimal amount);
    Task<bool> SendSubscriptionWarningEmailAsync(string toEmail, string toName, string companyName, DateTime expiryDate, int daysRemaining);
    Task<bool> SendSubscriptionExpiredEmailAsync(string toEmail, string toName, string companyName, DateTime expiredDate);
    Task<bool> SendPaymentReminderEmailAsync(string toEmail, string toName, string companyName, decimal amount, DateTime dueDate);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IBackgroundEmailService? _backgroundEmailService;

    public EmailService(
        IConfiguration configuration, 
        ILogger<EmailService> logger,
        IBackgroundEmailService? backgroundEmailService = null)
    {
        _configuration = configuration;
        _logger = logger;
        _backgroundEmailService = backgroundEmailService;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        try
        {
            // Validate configuration
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["EmailSettings:FromName"] ?? "PayroTech System";
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
            
            // Validate required settings
            if (string.IsNullOrEmpty(smtpServer) || 
                string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("SMTP configuration is incomplete. Cannot send email.");
                return false;
            }
            
            _logger.LogInformation("Attempting to send email from {FromEmail} to {ToEmail} via {SmtpServer}:{SmtpPort}", 
                fromEmail, toEmail, smtpServer, smtpPort);
            
            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true, // Always enforce TLS for SMTP
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15000 // Reduced to 15 seconds to fail faster
            };
            
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
            mailMessage.To.Add(new MailAddress(toEmail, toName));
            
            // Use async with cancellation token for better timeout handling
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await client.SendMailAsync(mailMessage, cts.Token);
            
            _logger.LogInformation("✅ Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "❌ SMTP error sending email to {Email}. SMTP Status: {Status}, Message: {Message}", 
                toEmail, ex.StatusCode, ex.Message);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("❌ Email sending to {Email} timed out after 15 seconds", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {Email}. Error: {Message}, Type: {Type}", 
                toEmail, ex.Message, ex.GetType().Name);
            return false;
        }
    }

    public void QueueEmailAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        if (_backgroundEmailService != null)
        {
            _backgroundEmailService.QueueEmail(toEmail, toName, subject, htmlContent);
            _logger.LogInformation("📧 Email queued for background sending to {Email}", toEmail);
        }
        else
        {
            _logger.LogWarning("⚠️ Background email service not available. Email to {Email} not queued.", toEmail);
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName, string companyName, string tempPassword)
    {
        var subject = "Welcome to PayroTech - Your Account is Ready!";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .content {{ color: #374151; line-height: 1.6; }}
        .credentials {{ background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .credentials p {{ margin: 8px 0; }}
        .credentials strong {{ color: #1e40af; }}
        .warning {{ background: #fef3c7; border: 1px solid #f59e0b; padding: 15px; border-radius: 8px; margin: 20px 0; color: #92400e; }}
        .btn {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; margin-top: 20px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
            <p style='color: #6b7280;'>Multi-Tenant Payroll ERP System</p>
        </div>
        <div class='content'>
            <h2>Welcome, {toName}!</h2>
            <p>Your PayroTech account for <strong>{companyName}</strong> has been created successfully.</p>
            <p>You can now access the system using the credentials below:</p>
            
            <div class='credentials'>
                <p><strong>📧 Email:</strong> {toEmail}</p>
                <p><strong>🔑 Temporary Password:</strong> {tempPassword}</p>
            </div>
            
            <div class='warning'>
                ⚠️ <strong>Important:</strong> You will be required to change your password after your first login. This is for your account's security.
            </div>
            
            <center>
                <a href='https://payrotech.com/Auth/Login' class='btn'>🔐 Login to PayroTech</a>
            </center>
            
            <p style='margin-top: 30px;'>As a Company Manager, you can:</p>
            <ul>
                <li>Create and manage HR and Accountant accounts</li>
                <li>Set up teams and departments</li>
                <li>Review and approve payroll budgets</li>
                <li>Monitor company reports and analytics</li>
            </ul>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public void QueueWelcomeEmailAsync(string toEmail, string toName, string companyName, string tempPassword)
    {
        var subject = "Welcome to PayroTech - Your Account is Ready!";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .content {{ color: #374151; line-height: 1.6; }}
        .credentials {{ background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .credentials p {{ margin: 8px 0; }}
        .credentials strong {{ color: #1e40af; }}
        .warning {{ background: #fef3c7; border: 1px solid #f59e0b; padding: 15px; border-radius: 8px; margin: 20px 0; color: #92400e; }}
        .btn {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; margin-top: 20px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
            <p style='color: #6b7280;'>Multi-Tenant Payroll ERP System</p>
        </div>
        <div class='content'>
            <h2>Welcome, {toName}!</h2>
            <p>Your PayroTech account for <strong>{companyName}</strong> has been created successfully.</p>
            <p>You can now access the system using the credentials below:</p>
            
            <div class='credentials'>
                <p><strong>📧 Email:</strong> {toEmail}</p>
                <p><strong>🔑 Temporary Password:</strong> {tempPassword}</p>
            </div>
            
            <div class='warning'>
                ⚠️ <strong>Important:</strong> You will be required to change your password after your first login. This is for your account's security.
            </div>
            
            <center>
                <a href='https://payrotech.com/Auth/Login' class='btn'>🔐 Login to PayroTech</a>
            </center>
            
            <p style='margin-top: 30px;'>As a Company Manager, you can:</p>
            <ul>
                <li>Create and manage HR and Accountant accounts</li>
                <li>Set up teams and departments</li>
                <li>Review and approve payroll budgets</li>
                <li>Monitor company reports and analytics</li>
            </ul>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        // Use QueueEmailAsync (background) instead of SendEmailAsync (direct/blocking)
        QueueEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public async Task<bool> SendPayslipEmailAsync(string toEmail, string toName, string payslipUrl, string period)
    {
        var subject = $"Your Payslip for {period} is Ready";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .content {{ color: #374151; line-height: 1.6; }}
        .btn {{ display: inline-block; background: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; margin-top: 20px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
        </div>
        <div class='content'>
            <h2>Hello, {toName}!</h2>
            <p>Your payslip for <strong>{period}</strong> is now available.</p>
            <p>You can view and download your payslip by clicking the button below:</p>
            
            <center>
                <a href='{payslipUrl}' class='btn'>📄 View Payslip</a>
            </center>
            
            <p style='margin-top: 20px;'>If you have any questions about your payslip, please contact your HR department or Accountant.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
    {
        var subject = "Reset Your PayroTech Password";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .btn {{ display: inline-block; background: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
        </div>
        <div class='content'>
            <h2>Password Reset Request</h2>
            <p>Hi {toName},</p>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            
            <center>
                <a href='{resetLink}' class='btn'>🔐 Reset Password</a>
            </center>
            
            <p style='margin-top: 20px; color: #6b7280;'>This link will expire in 24 hours. If you didn't request this, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public async Task<bool> SendPayrollApprovalNotificationAsync(string toEmail, string toName, string teamName, decimal amount)
    {
        var subject = $"Payroll Approved for {teamName}";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .amount {{ font-size: 32px; color: #059669; font-weight: bold; text-align: center; margin: 20px 0; }}
        .btn {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
        </div>
        <div class='content'>
            <h2>✅ Payroll Approved!</h2>
            <p>Hi {toName},</p>
            <p>The payroll budget for <strong>{teamName}</strong> has been approved and is ready for distribution:</p>
            
            <div class='amount'>₱{amount:N2}</div>
            
            <p>Please login to process and distribute the salaries to employees.</p>
            
            <center>
                <a href='https://payrotech.com/accountant/payroll' class='btn'>Process Payroll</a>
            </center>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public async Task<bool> SendSubscriptionWarningEmailAsync(string toEmail, string toName, string companyName, DateTime expiryDate, int daysRemaining)
    {
        var urgencyColor = daysRemaining <= 3 ? "#dc2626" : "#f59e0b";
        var urgencyIcon = daysRemaining <= 3 ? "⚠️" : "⏰";
        
        var subject = $"{urgencyIcon} Subscription Expiring Soon - {companyName}";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .warning-box {{ background: #fef3c7; border-left: 4px solid {urgencyColor}; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .warning-box h3 {{ color: {urgencyColor}; margin-top: 0; }}
        .expiry-date {{ font-size: 24px; color: {urgencyColor}; font-weight: bold; text-align: center; margin: 20px 0; }}
        .btn {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; margin-top: 20px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
            <p style='color: #6b7280;'>Subscription Management</p>
        </div>
        <div class='content'>
            <div class='warning-box'>
                <h3>{urgencyIcon} Subscription Expiring Soon!</h3>
                <p>Dear {toName},</p>
                <p>This is a reminder that your PayroTech subscription for <strong>{companyName}</strong> will expire in:</p>
                <div class='expiry-date'>{daysRemaining} Day{(daysRemaining != 1 ? "s" : "")}</div>
                <p><strong>Expiry Date:</strong> {expiryDate:MMMM dd, yyyy}</p>
            </div>
            
            <p>To avoid service interruption, please renew your subscription before the expiry date.</p>
            
            <h4>What happens if subscription expires?</h4>
            <ul>
                <li>❌ Access to the system will be suspended</li>
                <li>❌ Payroll processing will be disabled</li>
                <li>❌ Employee data will be temporarily inaccessible</li>
                <li>❌ Reports and analytics will be unavailable</li>
            </ul>
            
            <center>
                <a href='https://payrotech.com/subscription/renew' class='btn'>🔄 Renew Subscription Now</a>
            </center>
            
            <p style='margin-top: 30px; color: #6b7280; font-size: 14px;'>
                If you have already renewed, please disregard this message. For assistance, contact our support team.
            </p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
            <p>This is an automated reminder from PayroTech Subscription Management.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public async Task<bool> SendSubscriptionExpiredEmailAsync(string toEmail, string toName, string companyName, DateTime expiredDate)
    {
        var subject = $"🚫 Subscription Expired - {companyName}";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .expired-box {{ background: #fee2e2; border-left: 4px solid #dc2626; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .expired-box h3 {{ color: #dc2626; margin-top: 0; }}
        .btn {{ display: inline-block; background: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; margin-top: 20px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
            <p style='color: #6b7280;'>Subscription Management</p>
        </div>
        <div class='content'>
            <div class='expired-box'>
                <h3>🚫 Subscription Has Expired</h3>
                <p>Dear {toName},</p>
                <p>Your PayroTech subscription for <strong>{companyName}</strong> has expired on <strong>{expiredDate:MMMM dd, yyyy}</strong>.</p>
            </div>
            
            <h4>Current Status:</h4>
            <ul>
                <li>❌ System access is now suspended</li>
                <li>❌ Payroll processing is disabled</li>
                <li>❌ Employee data is temporarily locked</li>
                <li>❌ All features are unavailable</li>
            </ul>
            
            <p><strong>To restore access immediately:</strong></p>
            <ol>
                <li>Renew your subscription using the button below</li>
                <li>Complete the payment process</li>
                <li>Access will be restored within minutes</li>
            </ol>
            
            <center>
                <a href='https://payrotech.com/subscription/renew' class='btn'>🔄 Renew Subscription Now</a>
            </center>
            
            <p style='margin-top: 30px; background: #f3f4f6; padding: 15px; border-radius: 8px;'>
                <strong>💡 Note:</strong> Your data is safe and will be retained for 30 days. After 30 days of inactivity, data may be archived.
            </p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
            <p>Need help? Contact support@payrotech.com</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public async Task<bool> SendPaymentReminderEmailAsync(string toEmail, string toName, string companyName, decimal amount, DateTime dueDate)
    {
        var subject = $"💳 Payment Reminder - {companyName}";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 28px; font-weight: bold; }}
        .amount-box {{ background: #eff6ff; border: 2px solid #2563eb; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0; }}
        .amount {{ font-size: 36px; color: #2563eb; font-weight: bold; }}
        .btn {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; margin-top: 20px; }}
        .footer {{ text-align: center; color: #9ca3af; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>💼 PayroTech</div>
            <p style='color: #6b7280;'>Payment Reminder</p>
        </div>
        <div class='content'>
            <h2>💳 Payment Due Soon</h2>
            <p>Dear {toName},</p>
            <p>This is a friendly reminder that your subscription payment for <strong>{companyName}</strong> is due soon.</p>
            
            <div class='amount-box'>
                <p style='margin: 0; color: #6b7280;'>Amount Due</p>
                <div class='amount'>₱{amount:N2}</div>
                <p style='margin: 0; color: #6b7280;'>Due Date: {dueDate:MMMM dd, yyyy}</p>
            </div>
            
            <p>Please ensure payment is made before the due date to avoid service interruption.</p>
            
            <h4>Payment Methods:</h4>
            <ul>
                <li>💳 Credit/Debit Card</li>
                <li>🏦 Bank Transfer</li>
                <li>📱 GCash / PayMaya</li>
                <li>🌐 Online Banking</li>
            </ul>
            
            <center>
                <a href='https://payrotech.com/payment/process' class='btn'>💰 Make Payment Now</a>
            </center>
            
            <p style='margin-top: 30px; color: #6b7280; font-size: 14px;'>
                If you have already made the payment, please disregard this message. Payment processing may take 1-2 business days.
            </p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 PayroTech. All rights reserved.</p>
            <p>Questions? Contact billing@payrotech.com</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }

    public async Task<bool> SendStaffWelcomeEmailAsync(string toEmail, string toName, string companyName, string role, string tempPassword)
    {
        var roleColor = role switch
        {
            "HR"         => "#059669",
            "Accountant" => "#ea8a1a",
            _            => "#2563eb"
        };
        var subject = $"Welcome to PayroTech — Your {role} Account is Ready";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
  <style>
    body{{font-family:'Segoe UI',Arial,sans-serif;background:#f5f5f5;padding:20px;}}
    .wrap{{max-width:600px;margin:0 auto;background:#fff;border-radius:12px;padding:40px;box-shadow:0 4px 6px rgba(0,0,0,.1);}}
    .logo{{color:#2563eb;font-size:26px;font-weight:700;text-align:center;margin-bottom:4px;}}
    .role-badge{{display:inline-block;background:{roleColor};color:#fff;padding:4px 14px;border-radius:20px;font-size:13px;font-weight:600;margin-bottom:20px;}}
    .creds{{background:#f3f4f6;padding:20px;border-radius:8px;margin:20px 0;}}
    .creds p{{margin:8px 0;font-size:15px;}}
    .warn{{background:#fef3c7;border:1px solid #f59e0b;padding:14px;border-radius:8px;margin:20px 0;color:#92400e;font-size:14px;}}
    .steps{{background:#eff6ff;border:1px solid #3b82f6;padding:16px;border-radius:8px;margin:20px 0;}}
    .steps h4{{color:#1e40af;margin:0 0 10px;}}
    .steps ol{{margin:0;padding-left:20px;color:#1e40af;font-size:14px;}}
    .steps li{{margin-bottom:6px;}}
    .footer{{text-align:center;color:#9ca3af;font-size:12px;margin-top:30px;}}
  </style>
</head>
<body>
  <div class='wrap'>
    <div class='logo'>💼 PayroTech</div>
    <div style='text-align:center;margin-bottom:24px;'>
      <span class='role-badge'>{role}</span>
    </div>
    <h2 style='margin:0 0 8px;'>Welcome, {toName}!</h2>
    <p style='color:#6b7280;'>Your <strong>{role}</strong> account for <strong>{companyName}</strong> has been created.</p>
    <div class='creds'>
      <p><strong>📧 Email / Username:</strong> {toEmail}</p>
      <p><strong>🔑 Temporary Password:</strong> <code style='background:#e5e7eb;padding:2px 8px;border-radius:4px;font-size:15px;'>{tempPassword}</code></p>
    </div>
    <div class='warn'>
      ⚠️ <strong>You must change this password on first login.</strong> Keep it confidential until then.
    </div>
    <div class='steps'>
      <h4>📋 First Login Steps</h4>
      <ol>
        <li>Login with the credentials above</li>
        <li>Set a new secure password</li>
        <li>Capture your face photo (used for your ID card &amp; attendance)</li>
        <li>Print your ID card</li>
        <li>You're all set — access your dashboard!</li>
      </ol>
    </div>
    <div class='footer'>
      <p>&copy; 2026 PayroTech. All rights reserved.</p>
      <p>This is an automated message. Do not reply.</p>
    </div>
  </div>
</body>
</html>";
        return await SendEmailAsync(toEmail, toName, subject, htmlContent);
    }
}
