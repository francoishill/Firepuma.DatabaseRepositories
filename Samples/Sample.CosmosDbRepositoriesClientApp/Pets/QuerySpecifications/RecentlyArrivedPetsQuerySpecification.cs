using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Sample.CosmosDbRepositoriesClientApp.Pets.Entities;

namespace Sample.CosmosDbRepositoriesClientApp.Pets.QuerySpecifications;

public class RecentlyArrivedPetsQuerySpecification : QuerySpecification<PetEntity>
{
    public RecentlyArrivedPetsQuerySpecification(DateTime arrivedAfterDate)
    {
        WhereExpressions.Add(pet => pet.ArrivedOn >= arrivedAfterDate);
    }
}