using System.ComponentModel.DataAnnotations;

namespace backend.Models.Auth.DTOs;

public class EmailVerificationRequest
{
    [Required]
    public string Token {get; set;} = string.Empty;
}