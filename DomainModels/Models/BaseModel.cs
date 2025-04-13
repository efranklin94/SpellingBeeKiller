using MessagePack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using KeyAttribute = MessagePack.KeyAttribute;

namespace DomainModels.Models;

[MessagePackObject]
public class BaseDbModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Key(0)]
    public string? Id { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [Key(100)]
    public int OptimisticLockVersion { get; set; }
}
