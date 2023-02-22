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

    protected abstract string GenerateId(T entity);
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
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
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
        if (!string.IsNullOrWhiteSpace(item.Id))
        {
            throw new InvalidOperationException($"Item Id should be specified when calling CosmosDbRepository.AddItemAsync, it is auto-generated (item id {item.Id})");
        }

        item.Id = GenerateId(item);

        var response = await Container.CreateItemAsync<T>(item, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);

        Logger.LogInformation(
            "Added item id {Id} to container {Container}, which consumed {Charge} RUs",
            item.Id, Container.Id, response.RequestCharge);

        return response.Resource;
    }

    public async Task<T> UpsertItemAsync(
        T item,
        bool ignoreETag,
        CancellationToken cancellationToken = default)
    {
        var options = new ItemRequestOptions();

        if (!ignoreETag)
        {
            options.IfMatchEtag = item.ETag;
        }

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        item.Id ??= GenerateId(item);

        var response = await Container.UpsertItemAsync<T>(item, ResolvePartitionKey(item.Id), options, cancellationToken);

        Logger.LogInformation(
            "Upserted item id {Id} in container {Container}, which consumed {Charge} RUs",
            item.Id, Container.Id, response.RequestCharge);

        return response.Resource;
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

        var response = await Container.DeleteItemAsync<T>(item.Id, ResolvePartitionKey(item.Id), options, cancellationToken);

        Logger.LogInformation(
            "Deleted item id {Id} from container {Container}, which consumed {Charge} RUs",
            item.Id, Container.Id, response.RequestCharge);
    }

    protected virtual IQueryable<T> ApplyQuery(IQuerySpecification<T> querySpecification)
    {
        var evaluator = new CosmosDbQuerySpecificationEvaluator<T>();
        var inputQuery = Container.GetItemLinqQueryable<T>();
        return evaluator.GetQuery(inputQuery, querySpecification);
    }
}