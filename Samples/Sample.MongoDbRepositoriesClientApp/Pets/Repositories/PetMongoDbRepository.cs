using Firepuma.DatabaseRepositories.MongoDb.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Sample.MongoDbRepositoriesClientApp.Pets.Entities;

namespace Sample.MongoDbRepositoriesClientApp.Pets.Repositories;

public class PetMongoDbRepository : MongoDbRepository<PetEntity>, IPetRepository
{
    public PetMongoDbRepository(ILogger logger, IMongoCollection<PetEntity> collection)
        : base(logger, collection)
    {
    }

    public string GenerateId() => ObjectId.GenerateNewId().ToString();

    public async Task SetInsertedTimestampAsync(PetEntity item, CancellationToken cancellationToken)
    {
        var updateDefinition = Builders<PetEntity>.Update
            .CurrentDate(e => e.InsertedTimestamp);

        await UpdateItemAsync(item, updateDefinition, false, cancellationToken);
    }
}