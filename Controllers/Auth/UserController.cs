using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using backend.Models.Auth.DTOs;
using backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;

namespace backendv1.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class UserController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public UserController(AuthService authService, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _authService = authService;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("signup")]
    [EnableRateLimiting("SignupPolicy")]
    public async Task<IActionResult> Signup([FromForm] SignupRequest request)
    {
        try
        {
            SignupResponse response = await _authService.SignupAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }catch(InvalidOperationException exp)
        {
            return BadRequest(new
            {
                message = exp.Message
            });
        }
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            await _authService.VerifyEmailAsync(token);

            return Ok(new
            {
                message = "Email verified successfully. You can Login now."
            });
        }
        catch (ArgumentException exp)
        {
            return BadRequest(new
            {
                message = exp.Message
            });
        }catch(InvalidOperationException exp)
        {
            return BadRequest(new
            {
                message = exp.Message
            });
        }
    }

    // Login
    [HttpPost("signin")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            int refreshExpiryDays = int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out int days) && days > 0 ? days : 7;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,

                // Allow Http Only for local development
                // Secure = !_environment.IsDevelopment() || Request.IsHttps,
                // SameSite = SameSiteMode.Lax,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(refreshExpiryDays),
                Path = "/api/auth"
            };

            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            return Ok(result.Response);
        }catch(UnauthorizedAccessException exp)
        {
            return Unauthorized(new
            {
                message = exp.Message
            });
        }catch(InvalidOperationException exp)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = exp.Message
            });
        }catch(ArgumentException exp)
        {
            return BadRequest(new
            {
                message = exp.Message
            });
        }
    }

    // Forgot-Password
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("ForgotPasswordPolicy")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            await _authService.ForgotPasswordAsync(request);

            return Ok(new
            {
                message = "If an account exists with this email, you will receive a password reset link."
            });
        }
        catch (ArgumentException exp)
        {
            return BadRequest(new
            {
                message = exp.Message
            });
        }
    }

    // Reset-Password
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("ResetPasswordPolicy")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);

            return Ok(new
            {
                message = "Password reset successfully. You can now login with your new password."
            });
        }
        catch (ArgumentException exp)
        {
            return BadRequest(new
            {
                message = exp.Message
            });   
        }catch(InvalidOperationException exp)
        {
            return BadRequest(new
            {
                message = exp.Message
            });
        }
    }
}