using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Sample.MongoDbRepositoriesClientApp.Pets.Entities;

namespace Sample.MongoDbRepositoriesClientApp.Pets.QuerySpecifications;

public class AllPetsQuerySpecification : QuerySpecification<PetEntity>
{
}