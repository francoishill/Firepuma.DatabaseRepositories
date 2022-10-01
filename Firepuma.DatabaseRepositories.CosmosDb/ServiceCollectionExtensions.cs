using Firepuma.DatabaseRepositories.Abstractions.Entities;
using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.DatabaseRepositories.CosmosDb.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Firepuma.DatabaseRepositories.CosmosDb;

public static class ServiceCollectionExtensions
{
    public static void AddCosmosDbRepositories(
        this IServiceCollection services,
        Action<CosmosDbRepositoriesOptions> configureOptions)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.AddSingleton<Database>(s =>
        {
            var options = s.GetRequiredService<IOptions<CosmosDbRepositoriesOptions>>().Value;

            var client = new CosmosClient(options.ConnectionString);
            var database = client.GetDatabase(options.DatabaseId);

            return database;
        });
    }

    public static void AddCosmosDbRepository<TEntity, TInterface, TClass>(
        this IServiceCollection services,
        string containerName,
        Func<ILogger<TClass>, Container, TClass> classFactory)
        where TEntity : BaseEntity, new()
        where TInterface : class, IRepository<TEntity>
        where TClass : class, TInterface
    {
        services.AddSingleton<TInterface, TClass>(s =>
        {
            var logger = s.GetRequiredService<ILogger<TClass>>();

            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer(containerName);

            return classFactory(logger, container);
        });
    }
}