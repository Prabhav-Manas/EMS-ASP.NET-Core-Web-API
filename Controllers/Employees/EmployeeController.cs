using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Models.Employees.DTOs;
using backend.Services.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Employees;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    private string? GetLoggedInUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }

    // CREATE EMPLOYEE
    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        string? userId = GetLoggedInUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "User identity not found!"
            });
        }

        var employee = await _employeeService.CreateEmployeeAsync(request, userId);

        return CreatedAtAction(
            nameof(GetSingleEmployee),
            new{id = employee.Id},
            employee
        );
    }

    // FETCH ALL EMPLOYEES
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        string? userId = GetLoggedInUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "User identity not found!"
            });
        }

        var employees = await _employeeService.GetAllEmployeeAsync(userId);

        return Ok(employees);
    }

    // FETCH A SINGLE EMPLOYEE
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSingleEmployee(string id)
    {
        string? userId = GetLoggedInUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "User identity not found!"
            });
        }

        var employee = await _employeeService.GetSingleEmployeeAsync(id, userId);

        if(employee is null)
        {
            return NotFound(new
            {
                message = "Employee not found!"
            });
        }

        return Ok(employee);
    }

    // UPDATE A EMPLOYEE
    // [HttpPatch("{id}")]
    // public async Task<IActionResult> UpdateEmployee(string id, [FromBody] UpdateEmployeeRequest request)
    // {
    //     string? userId = GetLoggedInUserId();

    //     if (string.IsNullOrWhiteSpace(userId))
    //     {
    //         return Unauthorized(new
    //         {
    //             message = "User identity not found!"
    //         });
    //     }

    //     var employee = await _employeeService.UpdateEmployeeAsync(id, request, userId);

    //     if(employee is null)
    //     {
    //         return NotFound(new
    //         {
    //             message = "Employee not found!"
    //         });
    //     }

    //     return Ok(employee);
    // }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchEmployee(string id, [FromBody] PatchEmployeeRequest request)
    {
        string? userId = GetLoggedInUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "User identity not found!"
            });
        }

        var employee = await _employeeService.PatchEmployeeAsync(id, request, userId);

        if (employee is null)
        {
            return NotFound(new
            {
                message = "Employee not found!"
            });
        }

        return Ok(employee);
    }

    // DELETE EMPLOYEE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(string id)
    {
        string? userId = GetLoggedInUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "User identity not found!"
            });
        }

        bool isDeleted = await _employeeService.DeleteEmployeeAsync(id, userId);

        if (!isDeleted)
        {
            return NotFound(new
            {
                message = "Employee not found!"
            });
        }

        return Ok(new
        {
            message = "Employee deleted successfully!"
        });
    }
}