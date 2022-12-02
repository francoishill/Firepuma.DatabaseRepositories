using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;

#pragma warning disable CS8618

namespace Sample.MongoDbRepositoriesClientApp.Pets.Entities;

public class PetEntity : BaseMongoDbEntity
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required DateTime BornOn { get; set; }
    public required DateTime ArrivedOn { get; set; }
}