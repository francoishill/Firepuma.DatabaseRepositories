using Firepuma.DatabaseRepositories.Abstractions.Entities;

#pragma warning disable CS8618

namespace Sample.CosmosDbRepositoriesClientApp.Pets.Entities;

public class PetEntity : BaseEntity
{
    public string Type { get; set; }
    public string Name { get; set; }
    public DateTime BornOn { get; set; }
    public DateTime ArrivedOn { get; set; }
}