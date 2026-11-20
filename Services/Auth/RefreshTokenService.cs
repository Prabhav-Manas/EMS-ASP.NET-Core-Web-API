using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace backend.Services.Auth;

public class RefreshTokenService
{
    public string GenerateRefreshToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(64);
        return WebEncoders.Base64UrlEncode(tokenBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hashToken = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashToken);
    }
}