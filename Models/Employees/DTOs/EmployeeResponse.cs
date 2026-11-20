namespace backend.Models.Employees.DTOs;

public class EmployeeResponse
{
    public string Id {get; set;} = string.Empty;
    public string EmployeeCode {get; set;} = string.Empty;
    public string Fullname {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Phone {get; set;} = string.Empty;
    public string Department {get; set;} = string.Empty;
    public string Designation {get; set;} = string.Empty;
    public decimal Salary {get; set;}
    public DateTime DateOfJoiningUtc {get; set;}
    public bool IsActive {get; set;}
    public DateTime CreatedAtUtc {get; set;}
    public DateTime? UpdatedAtUtc {get; set;}
}