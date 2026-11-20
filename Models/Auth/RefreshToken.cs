using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Auth;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("userId")]
    public string UserId {get; set;} = string.Empty;

    [BsonElement("tokenHash")]
    public string TokenHash {get; set;} = string.Empty;

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc {get; set;}

    [BsonElement("expiresAtUtc")]
    public DateTime ExpiresAtUtc {get; set;}

    [BsonElement("revokedAtUtc")]
    public DateTime? RevokedAtUtc {get; set;}
}