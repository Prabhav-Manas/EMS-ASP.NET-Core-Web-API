using System.ComponentModel.DataAnnotations;

namespace backend.Models.Auth.DTOs;

public class ResetPasswordRequest
{
    [Required]
    public string Token {get; set;} = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword {get; set;} = string.Empty;
}