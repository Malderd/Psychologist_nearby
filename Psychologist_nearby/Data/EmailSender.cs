using System.Net;
using System.Net.Mail;

namespace Psychologist_nearby.Data;

public class EmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        try
        {
            // Читаем настройки из конфигурации (User Secrets или appsettings.json)
            var smtpServer = _configuration["Smtp:Server"] ?? "smtp.mail.ru";
            var smtpPortStr = _configuration["Smtp:Port"] ?? "587";
            int smtpPort = int.Parse(smtpPortStr);
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Password"];

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                _logger.LogError("SMTP User или Password не заданы в конфигурации (User Secrets).");
                throw new InvalidOperationException("Не заданы учетные данные для отправки почты.");
            }

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser, "Psychologist Nearby"),
                Subject = subject,
                Body = message,
                IsBodyHtml = false
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Письмо успешно отправлено на адрес {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке письма на адрес {Email}", toEmail);
            throw;
        }
    }
}