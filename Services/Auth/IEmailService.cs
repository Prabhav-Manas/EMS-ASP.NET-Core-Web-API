namespace backend.Services.Auth;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string fullname, string verificationUrl);

    Task SendPasswordResetAsync(string email, string fullname, string resetUrl);
}