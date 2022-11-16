using Firepuma.DatabaseRepositories.Abstractions.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Firepuma.DatabaseRepositories.MongoDb.Entities;

public class BaseMongoDbEntity : IEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonProperty(PropertyName = "_id")]
    public string Id { get; set; } = null!;

    [JsonProperty(PropertyName = "ETag")]
    public string? ETag { get; set; }

    public DateTime Timestamp => ObjectId.Parse(Id).CreationTime;
}