using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Sample.CosmosDbRepositoriesClientApp.Pets.Entities;

namespace Sample.CosmosDbRepositoriesClientApp.Pets.Repositories;

public interface IPetRepository : IRepository<PetEntity>
{
    string GenerateId(string petType);
}