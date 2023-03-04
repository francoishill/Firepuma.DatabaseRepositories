using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;
using MongoDB.Driver;

#pragma warning disable CS8618

namespace Sample.MongoDbRepositoriesClientApp.Pets.Entities;

public class PetEntity : BaseMongoDbEntity
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required DateTime BornOn { get; set; }
    public required DateTime ArrivedOn { get; set; }

    public DateTime? InsertedTimestamp { get; set; }

    public static IEnumerable<CreateIndexModel<PetEntity>> GetSchemaIndexes()
    {
        return new[]
        {
            new CreateIndexModel<PetEntity>(Builders<PetEntity>.IndexKeys.Combine(
                    Builders<PetEntity>.IndexKeys.Ascending(p => p.Type),
                    Builders<PetEntity>.IndexKeys.Ascending(p => p.Name),
                    Builders<PetEntity>.IndexKeys.Ascending(p => p.BornOn)
                ),
                new CreateIndexOptions<PetEntity>
                {
                    Unique = true,
                }),
        };
    }
}