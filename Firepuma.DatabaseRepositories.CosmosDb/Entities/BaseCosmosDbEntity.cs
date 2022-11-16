using Firepuma.DatabaseRepositories.Abstractions.Entities;
using Newtonsoft.Json;

namespace Firepuma.DatabaseRepositories.CosmosDb.Entities;

public class BaseCosmosDbEntity : IEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = null!;

    [JsonProperty(PropertyName = "_etag")]
    public string? ETag { get; set; }

    [JsonProperty(PropertyName = "_ts")]
    public long Timestamp { get; set; }
}