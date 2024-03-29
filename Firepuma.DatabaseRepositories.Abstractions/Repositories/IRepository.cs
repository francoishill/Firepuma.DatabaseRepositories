﻿using Firepuma.DatabaseRepositories.Abstractions.Entities;
using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;

// ReSharper disable UnusedMember.Global

namespace Firepuma.DatabaseRepositories.Abstractions.Repositories;

// Thanks for inspiration to: https://medium.com/swlh/clean-architecture-with-partitioned-repository-pattern-using-azure-cosmos-db-62241854cbc5
public interface IRepository<T> where T : IEntity
{
    Task<IEnumerable<T>> GetItemsAsync(
        IQuerySpecification<T> querySpecification,
        CancellationToken cancellationToken = default);

    Task<int> GetItemsCountAsync(
        IQuerySpecification<T> querySpecification,
        CancellationToken cancellationToken = default);

    Task<T?> GetItemOrDefaultAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<T?> GetItemOrDefaultAsync(
        IQuerySpecification<T> querySpecification,
        CancellationToken cancellationToken = default);

    Task<T> GetItemAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<T> GetItemAsync(
        IQuerySpecification<T> querySpecification,
        CancellationToken cancellationToken = default);

    Task<T> AddItemAsync(
        T item,
        CancellationToken cancellationToken = default);

    Task<T> ReplaceItemAsync(
        T item,
        bool ignoreETag = false,
        CancellationToken cancellationToken = default);

    Task<T> ReplaceItemAsync(
        T item,
        CancellationToken cancellationToken = default);

    Task DeleteItemAsync(
        T item,
        bool ignoreETag = false,
        CancellationToken cancellationToken = default);

    Task DeleteItemAsync(
        T item,
        CancellationToken cancellationToken = default);
}