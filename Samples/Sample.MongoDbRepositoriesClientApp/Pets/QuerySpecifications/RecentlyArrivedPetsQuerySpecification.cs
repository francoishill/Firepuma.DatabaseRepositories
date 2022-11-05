using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Sample.MongoDbRepositoriesClientApp.Pets.Entities;

namespace Sample.MongoDbRepositoriesClientApp.Pets.QuerySpecifications;

public class RecentlyArrivedPetsQuerySpecification : QuerySpecification<PetEntity>
{
    public RecentlyArrivedPetsQuerySpecification(DateTime arrivedAfterDate)
    {
        WhereExpressions.Add(pet => pet.ArrivedOn >= arrivedAfterDate);
    }
}