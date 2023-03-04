using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Sample.MongoDbRepositoriesClientApp.Pets.Entities;

namespace Sample.MongoDbRepositoriesClientApp.Pets.Repositories;

public interface IPetRepository : IRepository<PetEntity>
{
    string GenerateId();

    Task SetInsertedTimestampAsync(PetEntity item, CancellationToken cancellationToken);
}