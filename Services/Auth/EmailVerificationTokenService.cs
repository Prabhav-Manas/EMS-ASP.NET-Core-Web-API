using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace backend.Services.Auth;

public class EmailVerificationTokenService
{
    private readonly IConfiguration _configuration;

    public EmailVerificationTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateEmailVerificationToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
        return WebEncoders.Base64UrlEncode(tokenBytes);
    }

    public string HashEmailVerificationToken(string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashToken = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashToken);
    }

    public string BuildEmailVerificationUrl(string token)
    {
        string frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? throw new InvalidOperationException("Frontend base url is missing.");
        string emailVerificationPath = _configuration["Frontend:EmailVerificationPath"] ?? "/verify-email";

        string baseUrl = frontendBaseUrl.TrimEnd('/');
        string path = emailVerificationPath.TrimStart('/');

        return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}";
    }
}