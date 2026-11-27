using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Psychologist_nearby.Data;

namespace Psychologist_nearby.Tests;

public enum EmailFault
{
    None,
    SmtpError,
    Timeout
}

public sealed class EmailFaultInjector : EmailSender
{
    public EmailFault Mode { get; set; } = EmailFault.None;

    public int Attempts { get; private set; }

    public int SuccessfulSends { get; private set; }

    public EmailFaultInjector()
        : base(
            new ConfigurationBuilder().Build(),
            NullLogger<EmailSender>.Instance)
    {
    }

    public override Task SendEmailAsync(
        string toEmail,
        string subject,
        string message)
    {
        Attempts++;

        switch (Mode)
        {
            case EmailFault.SmtpError:
                throw new SmtpException(
                    "FI-01: Simulated SMTP failure");

            case EmailFault.Timeout:
                throw new TimeoutException(
                    "FI-02: Simulated email timeout");

            default:
                SuccessfulSends++;
                return Task.CompletedTask;
        }
    }
}