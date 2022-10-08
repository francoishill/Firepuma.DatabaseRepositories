using System.Net;
using Firepuma.DatabaseRepositories.CosmosDb.Services.Requests;
using Firepuma.DatabaseRepositories.CosmosDb.Services.Results;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

// ReSharper disable ArgumentsStyleNamedExpression

namespace Firepuma.DatabaseRepositories.CosmosDb.Services;

internal class CosmosDbAdminService : ICosmosDbAdminService
{
    private readonly ILogger<CosmosDbAdminService> _logger;
    private readonly Database _cosmosDb;

    public CosmosDbAdminService(
        ILogger<CosmosDbAdminService> logger,
        Database cosmosDb)
    {
        _logger = logger;
        _cosmosDb = cosmosDb;
    }

    public async Task<CreateContainersResult> CreateContainersIfNotExist(
        IEnumerable<ContainerSpecification> containersToCreate,
        CancellationToken cancellationToken)
    {
        var successfulContainers = new List<CreateContainersResult.SuccessfulContainerSummary>();
        var failedContainers = new List<CreateContainersResult.FailedContainerSummary>();
        foreach (var containerSpecification in containersToCreate)
        {
            var container = containerSpecification.ContainerProperties;
            var throughputProperties = containerSpecification.ThroughputProperties;
            var requestOptions = containerSpecification.RequestOptions;

            _logger.LogDebug(
                "Creating container {Container} with PartitionKeyPath {PartitionKeyPath} (throughput: {Throughput}, maxThroughput: {MaxThroughPut})",
                container.Id, container.PartitionKeyPath, throughputProperties?.Throughput, throughputProperties?.AutoscaleMaxThroughput);

            try
            {
                var containerResponse = await _cosmosDb.CreateContainerIfNotExistsAsync(
                    container,
                    throughputProperties: throughputProperties,
                    requestOptions: requestOptions,
                    cancellationToken: cancellationToken);

                var wasNewlyCreated = containerResponse.StatusCode == HttpStatusCode.Created;

                if (wasNewlyCreated)
                {
                    _logger.LogInformation(
                        "Successfully created container {Container} with PartitionKeyPath {PartitionKeyPath}",
                        container.Id, container.PartitionKeyPath);
                }
                else
                {
                    _logger.LogInformation(
                        "Container {Container} already existed with PartitionKeyPath {PartitionKeyPath}",
                        container.Id, container.PartitionKeyPath);
                }

                successfulContainers.Add(new CreateContainersResult.SuccessfulContainerSummary(container.Id));
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to create container {Container} with PartitionKeyPath {PartitionKeyPath}, error: {Error}, stack: {Stack}",
                    container.Id, container.PartitionKeyPath,
                    exception.Message, exception.StackTrace);

                failedContainers.Add(new CreateContainersResult.FailedContainerSummary(container.Id, exception.Message));
            }
        }

        return new CreateContainersResult
        {
            SuccessfulContainers = successfulContainers,
            FailedContainers = failedContainers,
        };
    }
}