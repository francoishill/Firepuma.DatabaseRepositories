using Firepuma.DatabaseRepositories.Abstractions.Entities;
using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Indexes;
using Firepuma.DatabaseRepositories.MongoDb.Configuration;
using Firepuma.DatabaseRepositories.MongoDb.Configuration.Helpers;
using Firepuma.DatabaseRepositories.MongoDb.Indexes;
using Firepuma.DatabaseRepositories.MongoDb.Indexes.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Firepuma.DatabaseRepositories.MongoDb;

public static class ServiceCollectionExtensions
{
    public static void AddMongoDbRepositories(
        this IServiceCollection services,
        Action<MongoDbRepositoriesOptions> configureOptions,
        ConventionPack? customConventionPack = null,
        bool validateOnStart = true,
        Action<ClusterBuilder>? configureClusterBuilder = null)
    {
        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        var optionsBuilder = services
            .AddOptions<MongoDbRepositoriesOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations();

        if (validateOnStart)
        {
            optionsBuilder.ValidateOnStart();
        }

        services.AddSingleton<IMongoDatabase>(s =>
        {
            var options = s.GetRequiredService<IOptions<MongoDbRepositoriesOptions>>().Value;

            var mongoUrl = new MongoUrl(options.ConnectionString);

            var database = MongoConfigurationHelper.ConfigureMongoDatabase(
                customConventionPack,
                mongoUrl,
                options.DatabaseName,
                configureClusterBuilder);
            return database;
        });

        services.AddTransient<IMongoIndexesApplier, MongoIndexesApplier>();
    }

    public static void AddMongoDbRepository<TEntity, TInterface, TClass>(
        this IServiceCollection services,
        string collectionName,
        Func<ILogger<TClass>, IMongoCollection<TEntity>, IServiceProvider, TClass> classFactory,
        Func<IEnumerable<CreateIndexModel<TEntity>>> indexesFactory)
        where TEntity : IEntity
        where TInterface : class, IRepository<TEntity>
        where TClass : class, TInterface
    {
        services.AddSingleton<TInterface, TClass>(s =>
        {
            var logger = s.GetRequiredService<ILogger<TClass>>();

            var database = s.GetRequiredService<IMongoDatabase>();
            var collection = database.GetCollection<TEntity>(collectionName);

            return classFactory(logger, collection, s);
        });

        services.AddTransient<IMongoCollectionIndexApplier>(s =>
        {
            var database = s.GetRequiredService<IMongoDatabase>();
            var collection = database.GetCollection<TEntity>(collectionName);

            return new MongoCollectionIndexApplier<TEntity>(collection, indexesFactory);
        });
    }
}