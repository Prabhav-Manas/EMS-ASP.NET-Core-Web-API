using System.ComponentModel.DataAnnotations;

namespace backend.Models.Auth.DTOs;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
}