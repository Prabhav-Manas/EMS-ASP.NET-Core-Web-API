using System.ComponentModel.DataAnnotations;

namespace backend.Models.Employees.DTOs;

public class UpdateEmployeeRequest
{
    [Required]
    [StringLength(20)]
    public string EmployeeCode {get; set;} = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Fullname {get; set;} = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email {get; set;} = string.Empty;

    [Required]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
    public string Phone {get; set;} = string.Empty;

    [Required]
    [StringLength(50)]
    public string Department {get; set;} = string.Empty;

    [Required]
    [StringLength(100)]
    public string Designation {get; set;} = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99")]
    public decimal Salary {get; set;}

    [Required]
    public DateTime DateOfJoiningUtc {get; set;}

    public bool IsActive {get; set;}
}