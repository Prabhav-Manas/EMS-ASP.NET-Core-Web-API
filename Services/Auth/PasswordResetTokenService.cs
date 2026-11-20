using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace backend.Services.Auth;

public class PasswordResetTokenService
{
    private readonly IConfiguration _configuration;

    public PasswordResetTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GeneratePasswordResetToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
        return WebEncoders.Base64UrlEncode(tokenBytes);
    }

    public string HashPasswordResetToken(string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    public string BuildResetPasswordUrl(string token)
    {
        string frontendUrl = _configuration["Frontend:BaseUrl"] ?? throw new InvalidOperationException("Frontend base url is missing.");
        string resetPasswordPath = _configuration["Frontend:PasswordResetPath"] ?? "/reset-password";

        string baseUrl = frontendUrl.TrimEnd('/');
        string path = resetPasswordPath.TrimStart('/');

        return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}";
    }
}