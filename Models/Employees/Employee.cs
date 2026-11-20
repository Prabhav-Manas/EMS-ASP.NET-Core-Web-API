using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backendv1.Models.Employees;

public class Employee
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("userId")]
    public string UserId {get; set;} = string.Empty;

    [BsonElement("employeeCode")]
    public string EmployeeCode {get; set;} = string.Empty;

    [BsonElement("fullname")]
    public string Fullname {get; set;} = string.Empty;

    [BsonElement("email")]
    public string Email {get; set;} = string.Empty;

    [BsonElement("phone")]
    public string Phone {get; set;} = string.Empty;

    [BsonElement("department")]
    public string Department {get; set;} = string.Empty;

    [BsonElement("designation")]
    public string Designation {get; set;} = string.Empty;

    [BsonElement("salary")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Salary {get; set;}

    [BsonElement("dateOfJoiningUtc")]
    public DateTime DateOfJoiningUtc {get; set;}

    [BsonElement("isActive")]
    public bool IsActive {get; set;} = true;

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc {get; set;}

    [BsonElement("updatedAtUtc")]
    public DateTime? UpdatedAtUtc {get; set;}
}