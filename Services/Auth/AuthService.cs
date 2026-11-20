using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

using backend.Models.Auth;
using backend.Models.Auth.DTOs;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace backend.Services.Auth;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<RefreshToken> _refreshTokens;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly EmailVerificationTokenService _emailVerificationTokenService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly ProfilePhotoService _profilePhotoService;
    private readonly PasswordResetTokenService _passwordResetTokenService;

    public AuthService(IMongoDatabase database, IPasswordHasher<User> passwordHasher, IEmailService emailService, JwtService jwtService, IConfiguration configuration, EmailVerificationTokenService emailVerificationTokenService, RefreshTokenService refreshTokenService, ProfilePhotoService profilePhotoService, PasswordResetTokenService passwordResetTokenService)
    {
        _users = database.GetCollection<User>("Users");
        _refreshTokens = database.GetCollection<RefreshToken>("RefreshTokens");
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _jwtService = jwtService;
        _configuration = configuration;
        _emailVerificationTokenService = emailVerificationTokenService;
        _refreshTokenService = refreshTokenService;
        _profilePhotoService = profilePhotoService;
        _passwordResetTokenService = passwordResetTokenService;
    }

    // Signup
    public async Task<SignupResponse> SignupAsync(SignupRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Fullname) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new InvalidOperationException("Invalid Fields. All Fields are required.");
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _users.Find((user) => user.Email == normalizedEmail).FirstOrDefaultAsync();

        if(existingUser is not null)
        {
            throw new InvalidOperationException("User already exists.");
        }

        // Profile Photo Upload
        string profilePhotoUrl = string.Empty;

        if(request.ProfilePhoto != null)
        {
            profilePhotoUrl = await _profilePhotoService.UploadAsync(request.ProfilePhoto);
        }

        // Create User
        var user = new User
        {
            Fullname = request.Fullname,
            Email = normalizedEmail,
            Phone = request.Phone,
            ProfilePhoto = profilePhotoUrl
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        // Generate Email Verification Token
        string verificationToken = _emailVerificationTokenService.GenerateEmailVerificationToken();
        user.EmailVerificationTokenHash = _emailVerificationTokenService.HashEmailVerificationToken(verificationToken);
        user.EmailVerificationTokenExpiresAtUtc = DateTime.UtcNow.AddHours(24);

        await _users.InsertOneAsync(user);

        // Build Verification URL
        string verificationUrl = _emailVerificationTokenService.BuildEmailVerificationUrl(verificationToken);

        // Sending Email Verification Url
        await _emailService.SendEmailVerificationAsync(user.Email, user.Fullname, verificationUrl);

        // Response
        return new SignupResponse
        {
            Message = "Signup successful. Please verify your email.",
            UserId = user.Id ?? string.Empty,
            Fullname = user.Fullname,
            Email = user.Email,
            ProfilePhoto = user.ProfilePhoto
        };
    }

    // Verify-Email
    public async Task VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Verification token is required");
        }

        // Hash the received token
        string tokenHash = _emailVerificationTokenService.HashEmailVerificationToken(token);

        DateTime now = DateTime.UtcNow;

        // Find and verify the user
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq((user) => user.EmailVerificationTokenHash, tokenHash),
            Builders<User>.Filter.Eq((user) => user.EmailVerified, false),
            Builders<User>.Filter.Gt((user) => user.EmailVerificationTokenExpiresAtUtc, now)
        );

        var update = Builders<User>.Update
        .Set((user) => user.EmailVerified, true)
        .Unset((user) => user.EmailVerificationTokenHash)
        .Unset((user) => user.EmailVerificationTokenExpiresAtUtc);

        var result = await _users.UpdateOneAsync(filter, update);

        if(result.ModifiedCount == 0)
        {
            throw new InvalidOperationException("Invalid, Expired or Already used verification token.");
        }
    }

    // LogIn
    public async Task<(LoginResponse Response, string RefreshToken)> LoginAsync(LoginRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and Password are rquired");
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _users.Find((user) => user.Email == normalizedEmail).FirstOrDefaultAsync();

        if(user is null)
        {
            throw new UnauthorizedAccessException("Invalid Email or Password");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if(passwordResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid Email or Password");
        }

        if (!user.EmailVerified)
        {
            throw new InvalidOperationException("Please verify your email first.");
        }

        // Rehash Password if the hashing algorithm has changed
        if(passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _users.ReplaceOneAsync((u) => u.Id == user.Id, user);
        }

        // Generate AccessToken
        string accessToken = _jwtService.GenerateAccessToken(user);

        // Generate RefreshToken
        string refreshToken = _refreshTokenService.GenerateRefreshToken();
        string refreshTokenHash = _refreshTokenService.HashRefreshToken(refreshToken);

        // Refresh Token Expiry
        if(!int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out int expiryDays) || expiryDays <= 0)
        {
            expiryDays = 7;
        }

        DateTime now = DateTime.UtcNow;
        DateTime refreshExpiry = now.AddDays(expiryDays);

        // Store refresh token hash in MongoDB
        var refreshTokenDocument = new RefreshToken
        {
            UserId = user.Id ?? throw new InvalidOperationException("UserId is missing."),
            TokenHash = refreshTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExpiry
        };

        await _refreshTokens.InsertOneAsync(refreshTokenDocument);


        // Access Token Expiry
        int accessTokenExpiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int minutes) && minutes > 0 ? minutes : 15;

        // Response
        var response = new LoginResponse
        {
            Message = "Login Successful.",
            AccessToken = accessToken,
            ExpiresIn = accessTokenExpiryMinutes * 60,
            UserId = user.Id ?? string.Empty,
            Fullname = user.Fullname,
            Email = user.Email
        };

        return (response, refreshToken);
    }

    // Forgot-Password
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _users.Find((user) => user.Email == normalizedEmail).FirstOrDefaultAsync();

        if(existingUser is null || !existingUser.EmailVerified)
        {
            return;
        }

        string resetPasswordToken = _passwordResetTokenService.GeneratePasswordResetToken();

        string resetPasswordTokenHash = _passwordResetTokenService.HashPasswordResetToken(resetPasswordToken);

        DateTime now = DateTime.UtcNow;
        DateTime expiry = now.AddMinutes(15);

        string resetPasswordUrl = _passwordResetTokenService.BuildResetPasswordUrl(resetPasswordToken);

        // Store hashed token and expiry in MongoDB
        var filter = Builders<User>.Filter.Eq(u => u.Id, existingUser.Id);

        var update = Builders<User>.Update.Set((u) => u.PasswordResetTokenHash, resetPasswordTokenHash).Set((u) => u.PasswordResetTokenExpiresAtUtc, expiry);

        await _users.UpdateOneAsync(filter, update);

        // Send Reset Password Email
        await _emailService.SendPasswordResetAsync(existingUser.Email, existingUser.Fullname, resetPasswordUrl);
    }

    // Reset-Password
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new ArgumentException("Reset token is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException("New Password is required.");
        }

        // Hash the original reset token
        string tokenHash = _passwordResetTokenService.HashPasswordResetToken(request.Token);

        DateTime now = DateTime.UtcNow;

        // Find user using tokenHash and expiry
        var filter = Builders<User>.Filter.And(Builders<User>.Filter.Eq((u) => u.PasswordResetTokenHash, tokenHash), Builders<User>.Filter.Gt((u) => u.PasswordResetTokenExpiresAtUtc, now));

        var existingUser = await _users.Find(filter).FirstOrDefaultAsync();

        if(existingUser is null)
        {
            throw new InvalidOperationException("Invalid or expired password reset token.");
        }

        // Hash New Password
        string newPasswordHash = _passwordHasher.HashPassword(existingUser, request.NewPassword);

        // Update Password and invalidate reset token
        var update = Builders<User>.Update.Set((u) => u.PasswordHash, newPasswordHash).Unset((u) => u.PasswordResetTokenHash).Unset((u) => u.PasswordResetTokenExpiresAtUtc);

        var result = await _users.UpdateOneAsync(filter, update);

        if(result.ModifiedCount == 0)
        {
            throw new InvalidOperationException("Invalid or expired password reset token.");
        }

        // Revoke all active refresh tokens belonging to this user
        var refreshTokenFilter = Builders<RefreshToken>.Filter.And(Builders<RefreshToken>.Filter.Eq((t) => t.UserId, existingUser.Id!), Builders<RefreshToken>.Filter.Eq((t) => t.RevokedAtUtc, null));

        var refreshTokenUpdate = Builders<RefreshToken>.Update.Set((t) => t.RevokedAtUtc, now);

        await _refreshTokens.UpdateManyAsync(refreshTokenFilter, refreshTokenUpdate);
    }
}