using Firepuma.DatabaseRepositories.CosmosDb.Repositories;
using Microsoft.Azure.Cosmos;
using Sample.CosmosDbRepositoriesClientApp.Pets.Entities;

namespace Sample.CosmosDbRepositoriesClientApp.Pets.Repositories;

public class PetCosmosDbRepository : CosmosDbRepository<PetEntity>, IPetRepository
{
    public PetCosmosDbRepository(ILogger logger, Container container)
        : base(logger, container)
    {
    }

    protected override string GenerateId(PetEntity entity) => $"{Guid.NewGuid().ToString()}:{entity.Type}";
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}