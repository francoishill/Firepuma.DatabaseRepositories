using Firepuma.DatabaseRepositories.CosmosDb.Services.Requests;
using Microsoft.Azure.Cosmos;

namespace Sample.CosmosDbRepositoriesClientApp.Configuration;

public static class CosmosContainers
{
    public static readonly ContainerSpecification Pets = new()
    {
        ContainerProperties = new ContainerProperties(id: "Pets", partitionKeyPath: "/Type"),

        //TODO: use ThroughputProperties.CreateAutoscaleThroughput() or ThroughputProperties.CreateManualThroughput() if Cosmos account is not serverless
        ThroughputProperties = null,
    };
}