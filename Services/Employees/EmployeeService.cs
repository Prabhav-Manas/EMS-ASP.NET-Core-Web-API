using backend.Models.Employees.DTOs;
using backendv1.Models.Employees;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.Services.Employees;

public class EmployeeService
{
    private readonly IMongoCollection<Employee> _employees;

    public EmployeeService(IMongoDatabase database)
    {
        _employees = database.GetCollection<Employee>("employees");
    }

    // CREATE EMPLOYEE
    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request, string userId)
    {
        var now = DateTime.UtcNow;

        var employee = new Employee
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            EmployeeCode = request.EmployeeCode.Trim(),
            Fullname = request.Fullname.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Department = request.Department.Trim(),
            Designation = request.Designation.Trim(),
            Salary = request.Salary,
            DateOfJoiningUtc = request.DateOfJoiningUtc,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = null
        };

        await _employees.InsertOneAsync(employee);
        return MapToResponse(employee);
    }

    // FETCH ALL EMPLOYEES FOR LOGGED-IN USER
    public async Task<List<EmployeeResponse>> GetAllEmployeeAsync(string userId)
    {
        var filter = Builders<Employee>.Filter.Eq((employee) => employee.UserId, userId);

        var employees = await _employees.Find(filter).SortByDescending((employee) => employee.CreatedAtUtc).ToListAsync();

        return employees.Select(MapToResponse).ToList();
    }

    // FETCH A SINGLE EMPLOYEE FOR LOGGED-IN USER
    public async Task<EmployeeResponse?> GetSingleEmployeeAsync(string id, string userId)
    {
        if(!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        var filter = Builders<Employee>.Filter.And(
            Builders<Employee>.Filter.Eq((employee) => employee.Id, id),
            Builders<Employee>.Filter.Eq((employee) => employee.UserId, userId)
        );

        var employee = await _employees.Find(filter).FirstOrDefaultAsync();

        return employee is null ? null : MapToResponse(employee);
    }

    // UPDATE EMPLOYEE FOR LOGGED-IN USER
    // public async Task<EmployeeResponse?> UpdateEmployeeAsync(string id, UpdateEmployeeRequest request, string userId)
    // {
    //     if(!ObjectId.TryParse(id, out _))
    //     {
    //         return null;
    //     }

    //     var filter = Builders<Employee>.Filter.And(
    //         Builders<Employee>.Filter.Eq((employee) => employee.Id, id),
    //         Builders<Employee>.Filter.Eq((employee) => employee.UserId, userId)
    //     );

    //     var update = Builders<Employee>.Update
    //         .Set((employee) => employee.EmployeeCode, request.EmployeeCode.Trim())
    //         .Set((employee) => employee.Fullname, request.Fullname.Trim())
    //         .Set((employee) => employee.Email, request.Email.Trim())
    //         .Set((employee) => employee.Phone, request.Phone.Trim())
    //         .Set((employee) => employee.Department, request.Department.Trim())
    //         .Set((employee) => employee.Designation, request.Designation.Trim())
    //         .Set((employee) => employee.Salary, request.Salary)
    //         .Set((employee) => employee.DateOfJoiningUtc, request.DateOfJoiningUtc)
    //         .Set((employee) => employee.IsActive, request.IsActive)
    //         .Set((employee) => employee.UpdatedAtUtc, DateTime.UtcNow);

    //     var updatedEmployee = await _employees.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Employee>{ReturnDocument = ReturnDocument.After});

    //     return updatedEmployee is null ? null : MapToResponse(updatedEmployee);
    // }


    public async Task<EmployeeResponse?> PatchEmployeeAsync(string id, PatchEmployeeRequest request, string userId)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        var filter = Builders<Employee>.Filter.And(
            Builders<Employee>.Filter.Eq(employee => employee.Id, id),
            Builders<Employee>.Filter.Eq(employee => employee.UserId, userId)
        );

        var updates = new List<UpdateDefinition<Employee>>();

        if (request.EmployeeCode is not null)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.EmployeeCode,
                request.EmployeeCode.Trim()
            ));

        if (request.Fullname is not null)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.Fullname,
                request.Fullname.Trim()
            ));

        if (request.Email is not null)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.Email,
                request.Email.Trim()
            ));

        if (request.Phone is not null)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.Phone,
                request.Phone.Trim()
            ));

        if (request.Department is not null)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.Department,
                request.Department.Trim()
            ));

        if (request.Designation is not null)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.Designation,
                request.Designation.Trim()
            ));

        if (request.Salary.HasValue)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.Salary,
                request.Salary.Value
            ));

        if (request.DateOfJoiningUtc.HasValue)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.DateOfJoiningUtc,
                request.DateOfJoiningUtc.Value
            ));

        if (request.IsActive.HasValue)
            updates.Add(Builders<Employee>.Update.Set(
                employee => employee.IsActive,
                request.IsActive.Value
            ));

        // No fields supplied: return the existing employee without updating it.
        if (updates.Count == 0)
        {
            var existingEmployee = await _employees.Find(filter).FirstOrDefaultAsync();

            return existingEmployee is null
                ? null
                : MapToResponse(existingEmployee);
        }

        updates.Add(Builders<Employee>.Update.Set(
            employee => employee.UpdatedAtUtc,
            DateTime.UtcNow
        ));

        var combinedUpdate = Builders<Employee>.Update.Combine(updates);

        var updatedEmployee = await _employees.FindOneAndUpdateAsync(
            filter,
            combinedUpdate,
            new FindOneAndUpdateOptions<Employee>
            {
                ReturnDocument = ReturnDocument.After
            }
        );

        return updatedEmployee is null
            ? null
            : MapToResponse(updatedEmployee);
    }


    // DELETE EMPLOYEE FOR LOGGED-IN USER
    public async Task<bool> DeleteEmployeeAsync(string id, string userId)
    {
        if(!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var filter = Builders<Employee>.Filter.And(
            Builders<Employee>.Filter.Eq((employee) => employee.Id, id),
            Builders<Employee>.Filter.Eq((employee) => employee.UserId, userId)
        );

        var result = await _employees.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }

    // MAP EMPLOYEE MODEL TO RESPONSE DTO
    private static EmployeeResponse MapToResponse(Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id!,
            EmployeeCode = employee.EmployeeCode,
            Fullname = employee.Fullname,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.Department,
            Designation = employee.Designation,
            Salary = employee.Salary,
            DateOfJoiningUtc = employee.DateOfJoiningUtc,
            IsActive = employee.IsActive,
            CreatedAtUtc = employee.CreatedAtUtc,
            UpdatedAtUtc = employee.UpdatedAtUtc
        };
    }
}