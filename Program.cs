using MongoDB.Driver;
using backend.Data;
using backend.Models.Auth;
using backend.Models.Auth.DTOs;
using backend.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using backend.Services.Employees;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<EmailVerificationTokenService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<ProfilePhotoService>();
builder.Services.AddScoped<PasswordResetTokenService>();
builder.Services.AddScoped<EmployeeService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer((options) =>
{
    string secretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt secret key is missing.");
    string issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt issuer is missing.");
    string audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt audience is missing.");

    byte[] keyBytes = Convert.FromBase64String(secretKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
    };
});

builder.Services.AddAuthorization();


builder.Services.AddRateLimiter((options) => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Signup
    options.AddPolicy("SignupPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey:context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }
    ));

    // Login
    options.AddPolicy("LoginPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey:context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory:_ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }
    ));

    // Forgot-Password
    options.AddPolicy("ForgotPasswordPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey:context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory:_ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
            AutoReplenishment = true
        }
    ));

    // Reset-Password
    options.AddFixedWindowLimiter("ResetPasswordPolicy", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(5);
        options.QueueLimit = 0;
    });

    // Response when requests exceeds limit
    options.OnRejected = async(context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        if(context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();
        }

        await response.WriteAsJsonAsync(new
        {
            message = "Too many requests. Please try again later."
        }, cancellationToken);
    };
});

builder.Services.AddCors((options) =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// MongoDB Configuration
var mongoConfig = builder.Configuration.GetSection("MongoDb").Get<MongoDbConfiguration>() ?? throw new InvalidOperationException("MongoDB configuration is missing.");

if (string.IsNullOrWhiteSpace(mongoConfig.ConnectionString) || string.IsNullOrWhiteSpace(mongoConfig.DatabaseName))
{
    throw new InvalidOperationException("MongoDB connection string or database name is missing.");
}

// MongoDB Dependency Injection
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConfig.ConnectionString));

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();

    return client.GetDatabase(mongoConfig.DatabaseName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();

app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();
