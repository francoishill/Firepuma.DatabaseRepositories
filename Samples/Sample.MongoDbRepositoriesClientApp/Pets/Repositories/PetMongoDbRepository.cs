using Firepuma.DatabaseRepositories.MongoDb.Repositories;
using MongoDB.Driver;
using Sample.MongoDbRepositoriesClientApp.Pets.Entities;

namespace Sample.MongoDbRepositoriesClientApp.Pets.Repositories;

public class PetMongoDbRepository : MongoDbRepository<PetEntity>, IPetRepository
{
    public PetMongoDbRepository(ILogger logger, IMongoCollection<PetEntity> collection)
        : base(logger, collection)
    {
    }
}