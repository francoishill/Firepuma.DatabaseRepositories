using Firepuma.DatabaseRepositories.CosmosDb.Services.Requests;
using Firepuma.DatabaseRepositories.CosmosDb.Services.Results;

namespace Firepuma.DatabaseRepositories.CosmosDb.Services;

public interface ICosmosDbAdminService
{
    Task<CreateContainersResult> CreateContainersIfNotExist(
        IEnumerable<ContainerSpecification> containersToCreate,
        CancellationToken cancellationToken);
}