using Newtonsoft.Json;

namespace Firepuma.DatabaseRepositories.Abstractions.Entities;

public abstract class BaseEntity
{
    [JsonProperty(PropertyName = "id")]
    public virtual string Id { get; set; } = null!;

    [JsonProperty(PropertyName = "_etag")]
    public virtual string? ETag { get; set; }

    [JsonProperty(PropertyName = "_ts")]
    public virtual long Timestamp { get; set; }
}