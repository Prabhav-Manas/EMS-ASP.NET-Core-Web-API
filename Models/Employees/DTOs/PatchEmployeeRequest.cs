namespace backend.Models.Employees.DTOs;

public class PatchEmployeeRequest
{
    public string? EmployeeCode { get; set; }

    public string? Fullname { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Department { get; set; }

    public string? Designation { get; set; }

    public decimal? Salary { get; set; }

    public DateTime? DateOfJoiningUtc { get; set; }

    public bool? IsActive { get; set; }
}