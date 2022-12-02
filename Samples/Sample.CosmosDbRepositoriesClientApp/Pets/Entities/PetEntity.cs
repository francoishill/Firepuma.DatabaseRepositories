using Firepuma.DatabaseRepositories.CosmosDb.Abstractions.Entities;

#pragma warning disable CS8618

namespace Sample.CosmosDbRepositoriesClientApp.Pets.Entities;

public class PetEntity : BaseCosmosDbEntity
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required DateTime BornOn { get; set; }
    public required DateTime ArrivedOn { get; set; }
}