using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Sample.CosmosDbRepositoriesClientApp.Pets.Entities;

namespace Sample.CosmosDbRepositoriesClientApp.Pets.QuerySpecifications;

public class AllPetsQuerySpecification : QuerySpecification<PetEntity>
{
}