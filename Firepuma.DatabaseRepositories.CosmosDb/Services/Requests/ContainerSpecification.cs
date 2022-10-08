using Microsoft.Azure.Cosmos;

namespace Firepuma.DatabaseRepositories.CosmosDb.Services.Requests;

public class ContainerSpecification
{
    public ContainerProperties ContainerProperties { get; init; }
    public ThroughputProperties ThroughputProperties { get; init; }
    public RequestOptions RequestOptions { get; init; }
}