using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MindCheck.Application.Abstractions;

namespace MindCheck.Api.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> TrySendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken)
    {
        if (!_options.IsConfigured)
        {
            return false;
        }

        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_options.Username, _options.Password),
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = "รีเซ็ตรหัสผ่าน MindCheck",
                Body =
                    "มีคำขอรีเซ็ตรหัสผ่านสำหรับบัญชี MindCheck ของคุณ\n\n" +
                    $"กดลิงก์นี้เพื่อตั้งรหัสผ่านใหม่ (หมดอายุใน 1 ชั่วโมง):\n{resetLink}\n\n" +
                    "ถ้าคุณไม่ได้เป็นคนขอ สามารถเพิกเฉยต่ออีเมลนี้ได้เลย\n\n" +
                    "— MindCheck (โปรเจกต์สาธิต)",
                IsBodyHtml = false,
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            // Never let a broken mail server take down the forgot-password
            // request — fall back to the in-app link instead.
            _logger.LogWarning(ex, "Failed to send password reset email to {Email}", toEmail);
            return false;
        }
    }
}
