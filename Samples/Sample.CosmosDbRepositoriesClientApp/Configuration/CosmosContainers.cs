using Microsoft.Azure.Cosmos;

namespace Sample.CosmosDbRepositoriesClientApp.Configuration;

public static class CosmosContainers
{
    public static readonly ContainerProperties Pets = new(id: "Pets", partitionKeyPath: "/Type");
}