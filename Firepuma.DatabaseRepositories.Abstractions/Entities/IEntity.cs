namespace Firepuma.DatabaseRepositories.Abstractions.Entities;

public interface IEntity
{
    string Id { get; set; }
    string? ETag { get; set; }
}