using System.ComponentModel.DataAnnotations;

namespace backend.Models.Auth.DTOs;

public class SignupRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Fullname {get; set;} = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email {get; set;} = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password {get; set;} = string.Empty;

    [Required]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
    public string Phone {get; set;} = string.Empty;
    
    public IFormFile? ProfilePhoto {get; set;}
}