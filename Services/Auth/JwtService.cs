using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using backend.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.Auth;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        string secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt Secret Key is missing.");
        string issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt issuer is missing");
        string audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt audience is missing.");
        // int expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");

        if(!int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int expiryMinutes) || expiryMinutes <= 0)
        {
            expiryMinutes = 15;
        }

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(secretKey);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Jwt secret key must be a valid Base64 string.");
        }

        if(keyBytes.Length < 32)
        {
            throw new InvalidOperationException("Jwt secret key must contain at least 32 bytes");
        }

        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claim = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Fullname),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:issuer,
            audience:audience,
            claims:claim,
            expires:DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials:credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}