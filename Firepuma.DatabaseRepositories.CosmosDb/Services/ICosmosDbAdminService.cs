using Firepuma.DatabaseRepositories.CosmosDb.Services.Results;
using Microsoft.Azure.Cosmos;

namespace Firepuma.DatabaseRepositories.CosmosDb.Services;

public interface ICosmosDbAdminService
{
    Task<CreateContainersResult> CreateContainersIfNotExist(
        IEnumerable<ContainerProperties> containersToCreate,
        CancellationToken cancellationToken);
}