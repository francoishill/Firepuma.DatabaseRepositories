using System.Net;
using Firepuma.DatabaseRepositories.Abstractions.Exceptions;
using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.DatabaseRepositories.Abstractions.Repositories.Exceptions;
using Firepuma.DatabaseRepositories.CosmosDb.Abstractions.Entities;
using Firepuma.DatabaseRepositories.CosmosDb.Queries;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Firepuma.DatabaseRepositories.CosmosDb.Repositories;

public abstract class CosmosDbRepository<T> : IRepository<T> where T : BaseCosmosDbEntity
{
    protected readonly ILogger Logger;
    protected readonly Container Container;

    protected CosmosDbRepository(
        ILogger logger,
        Container container)
    {
        Logger = logger;
        Container = container;
    }

    protected abstract PartitionKey ResolvePartitionKey(string entityId);

    public async Task<IEnumerable<T>> GetItemsAsync(
        IQuerySpecification<T> querySpecification,
        CancellationToken cancellationToken)
    {
        var queryable = ApplyQuery(querySpecification);
        var iterator = queryable.ToFeedIterator<T>();

        var totalRequestCharge = 0D;

        var results = new List<T>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            results.AddRange(response.ToList());

            Logger.LogDebug(
                "Fetched {Count} items from container {Container}, which consumed {Charge} RUs",
                response.Count, Container.Id, response.RequestCharge);

            totalRequestCharge += response.RequestCharge;
        }

        Logger.LogDebug(
            "A total of {Count} items were fetched from container {Container} and consumed total {Charge} RUs",
            results.Count, Container.Id, totalRequestCharge);

        return results;
    }

    public async Task<int> GetItemsCountAsync(
        IQuerySpecification<T> querySpecification,
        CancellationToken cancellationToken)
    {
        var queryable = ApplyQuery(querySpecification);

        var response = await queryable.CountAsync(cancellationToken: cancellationToken);

        Logger.LogDebug(
            "Counted items from container {Container}, which consumed {Charge} RUs",
            Container.Id, response.RequestCharge);

        return response.Resource;
    }

    public async Task<T?> GetItemOrDefaultAsync(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await Container.ReadItemAsync<T>(id, ResolvePartitionKey(id), cancellationToken: cancellationToken);

            Logger.LogDebug(
                "Fetched item id {Id} from container {Container}, which consumed {Charge} RUs",
                id, Container.Id, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public async Task<T?> GetItemOrDefaultAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        //TODO: find a more optimized way than downloading Items and checking for Count>1 afterwards
        var items = (await GetItemsAsync(querySpecification, cancellationToken)).ToList();

        if (items.Count > 1)
        {
            var queryType = querySpecification.GetType();
            var queryTypeName = queryType.FullName ?? queryType.AssemblyQualifiedName;
            throw new MultipleResultsInsteadOfSingleException($"Query type: {queryTypeName}");
        }

        return items.FirstOrDefault();
    }

    public async Task<T> GetItemAsync(string id, CancellationToken cancellationToken = default)
    {
        var itemOrDefault = await GetItemOrDefaultAsync(id, cancellationToken);

        if (itemOrDefault == null)
        {
            throw new DatabaseItemNotFoundException(typeof(T), id);
        }

        return itemOrDefault;
    }

    public async Task<T> GetItemAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        var itemOrDefault = await GetItemOrDefaultAsync(querySpecification, cancellationToken);

        if (itemOrDefault == null)
        {
            throw new DatabaseItemNotFoundException(typeof(T), $"query type {querySpecification.GetType().Name}");
        }

        return itemOrDefault;
    }

    public async Task<T> AddItemAsync(
        T item,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            throw new InvalidOperationException($"Item Id is required to be non-empty before calling CosmosDbRepository.AddItemAsync");
        }

        Logger.LogDebug(
            "Will now add item id {Id} to container {Container}",
            item.Id, Container.Id);

        try
        {
            var response = await Container.CreateItemAsync<T>(item, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);

            Logger.LogInformation(
                "Added item id {Id} to container {Container}, which consumed {Charge} RUs",
                item.Id, Container.Id, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.Conflict)
        {
            //https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.client.documentclient.upsertdocumentasync?view=azure-dotnet
            throw new DuplicateDatabaseEntityException($"Duplicate id/key detected, unable to insert item with Id {item.Id}", cosmosException);
        }
    }

    public async Task<T> ReplaceItemAsync(
        T item,
        bool ignoreETag,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            throw new InvalidOperationException($"Item Id is required to be non-empty before calling CosmosDbRepository.ReplaceItemAsync");
        }

        var options = new ItemRequestOptions();

        if (!ignoreETag)
        {
            options.IfMatchEtag = item.ETag;
        }

        Logger.LogDebug(
            "Will now replace item id {Id} in container {Container}",
            item.Id, Container.Id);

        try
        {
            var response = await Container.ReplaceItemAsync<T>(item, item.Id, ResolvePartitionKey(item.Id), options, cancellationToken);

            Logger.LogInformation(
                "Replace item id {Id} in container {Container}, which consumed {Charge} RUs",
                item.Id, Container.Id, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            Logger.LogWarning(
                "Failed to replace item id {Id} in container {Container} due to PreconditionFailed response status but it consumed {Charge} RUs",
                item.Id, Container.Id, cosmosException.RequestCharge);

            throw new DocumentETagMismatchException();
        }
    }

    public async Task<T> ReplaceItemAsync(
        T item,
        CancellationToken cancellationToken = default)
    {
        return await ReplaceItemAsync(item, ignoreETag: false, cancellationToken);
    }

    public async Task DeleteItemAsync(
        T item,
        bool ignoreETag,
        CancellationToken cancellationToken)
    {
        if (item.Id == null) throw new ArgumentException("Item Id should not be null", nameof(item));

        var options = new ItemRequestOptions();

        if (!ignoreETag)
        {
            options.IfMatchEtag = item.ETag;
        }

        Logger.LogDebug(
            "Will now delete item id {Id} from container {Container}",
            item.Id, Container.Id);

        try
        {
            var response = await Container.DeleteItemAsync<T>(item.Id, ResolvePartitionKey(item.Id), options, cancellationToken);

            Logger.LogInformation(
                "Deleted item id {Id} from container {Container}, which consumed {Charge} RUs",
                item.Id, Container.Id, response.RequestCharge);
        }
        catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            Logger.LogWarning(
                "Failed to delete id {Id} from container {Container} due to PreconditionFailed response status but it consumed {Charge} RUs",
                item.Id, Container.Id, cosmosException.RequestCharge);

            throw new DocumentETagMismatchException();
        }
    }

    public async Task DeleteItemAsync(
        T item,
        CancellationToken cancellationToken)
    {
        await DeleteItemAsync(item, ignoreETag: false, cancellationToken);
    }

    protected virtual IQueryable<T> ApplyQuery(IQuerySpecification<T> querySpecification)
    {
        var evaluator = new CosmosDbQuerySpecificationEvaluator<T>();
        var inputQuery = Container.GetItemLinqQueryable<T>();
        return evaluator.GetQuery(inputQuery, querySpecification);
    }
}