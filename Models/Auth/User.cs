using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Auth;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("fullname")]
    public string Fullname {get; set;} = string.Empty;

    [BsonElement("email")]
    public string Email {get; set;} = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash {get; set;} = string.Empty;

    [BsonElement("phone")]
    public string Phone {get; set;} = string.Empty;

    [BsonElement("profilePhoto")]
    public string ProfilePhoto {get; set;} = string.Empty;

    [BsonElement("emailVerified")]
    public bool EmailVerified {get; set;} = false;

    [BsonElement("emailVerificationTokenHash")]
    public string? EmailVerificationTokenHash {get; set;}

    [BsonElement("emailVerificationTokenExpiresAtUtc")]
    public DateTime? EmailVerificationTokenExpiresAtUtc {get; set;}

    [BsonElement("passwordResetToken")]
    public string? PasswordResetTokenHash {get; set;}

    [BsonElement("passwordResetTokenExpiresAtUtc")]
    public DateTime? PasswordResetTokenExpiresAtUtc {get; set;}
}