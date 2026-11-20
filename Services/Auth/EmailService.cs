using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace backend.Services.Auth;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;    
    }

    public async Task SendEmailVerificationAsync(string email, string fullname, string verificationUrl)
    {
        string host = _configuration["Email:Host"] ?? throw new InvalidOperationException("Email SMTP host is missing.");
        string username = _configuration["Email:Username"] ?? throw new InvalidOperationException("Email username is missing.");
        string password = _configuration["Email:Password"] ?? throw new InvalidOperationException("Email password is missing.");
        string fromEmail = _configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Sender email is missing");
        string fromName = _configuration["Email:FromName"] ?? "DotNetApp";

        if(!int.TryParse(_configuration["Email:Port"], out int port) || port <= 0)
        {
            throw new InvalidOperationException("Invalid SMTP Port.");
        }

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(email));

        message.Subject = "Verify your email address.";

        string safeName = System.Net.WebUtility.HtmlEncode(fullname);
        string safeUrl = System.Net.WebUtility.HtmlEncode(verificationUrl);

        message.Body = new BodyBuilder
        {
            HtmlBody = $"""
                <html>
                    <body>
                        <h2>Hello {safeName},</h2>

                        <p>Thank you for signing up.</p>

                        <p>Please click the button below to verify your email address:</p>

                        <a href="{safeUrl}" style="background-color:#2563eb; color:white; padding:12px 20px; text-decoration:none; border-radius:5px;">
                            Verify Email
                        </a>

                        <p>This verification link expires in 24 hours.</p>

                        <p>If you did not create this account, please ignore this email.</p>
                    </body>
                </html>
            """,

            TextBody = $"""
                Hello {fullname},

                Thank you for signing up.

                Verify your email address using this link:

                {verificationUrl}

                This link expires in 24 hours.
            """
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(username, password);

        await smtp.SendAsync(new SmtpSendRequest(message), CancellationToken.None);

        await smtp.DisconnectAsync(true);
    }

    public async Task SendPasswordResetAsync(string email, string fullname, string resetUrl)
    {
        string host = _configuration["Email:Host"] ?? throw new InvalidOperationException("Email SMTP host is missing.");
        string username = _configuration["Email:Username"] ?? throw new InvalidOperationException("Email username is missing");
        string password = _configuration["Email:Password"] ?? throw new InvalidOperationException("Email password is missing.");
        string fromEmail = _configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Sender email is missing.");
        string fromName = _configuration["Email:FromName"] ?? "DotNetApp";

        if(!int.TryParse(_configuration["Email:Port"], out int port) || port <= 0)
        {
            throw new InvalidOperationException("Invalid SMTP port.");
        }

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(email));

        message.Subject = "Reset Your Password";

        string safeName = System.Net.WebUtility.HtmlEncode(fullname);
        string safeUrl = System.Net.WebUtility.HtmlEncode(resetUrl);

        message.Body = new BodyBuilder
        {
            HtmlBody = $"""
                <html>
                    <body>
                        <h2>Hello {safeName},</h2>

                        <p>We received a request to reset your password.</p>

                        <p>Click the button below to reset your password:</p>

                        <a href="{safeUrl}"
                        style="background-color:#2563eb; color:white;
                        padding:12px 20px; text-decoration:none;
                        border-radius:5px;">
                            Reset Password
                        </a>

                        <p>This link expires in 15 minutes.</p>

                        <p>If you did not request a password reset,
                        please ignore this email.</p>
                    </body>
                </html>
            """,

            TextBody = $"""
                Hello {fullname},

                We received a request to reset your password.

                Use this link to reset your password:

                {resetUrl}

                This link expires in 15 minutes.

                If you did not request this, please ignore this email.
            """
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(username, password);
        await smtp.SendAsync(new SmtpSendRequest(message), CancellationToken.None);
        await smtp.DisconnectAsync(true);
    }
}