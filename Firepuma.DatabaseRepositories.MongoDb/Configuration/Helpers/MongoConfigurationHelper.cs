using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Firepuma.DatabaseRepositories.MongoDb.Configuration.Helpers;

internal static class MongoConfigurationHelper
{
    public static IMongoDatabase ConfigureMongoDatabase(
        ConventionPack? customConventionPack,
        MongoUrl mongoUrl,
        string databaseName)
    {
        if (!string.IsNullOrWhiteSpace(mongoUrl.DatabaseName))
        {
            throw new InvalidOperationException(
                $"Please ensure no database is contained as part of the mongoUrl, database should be specified with the {nameof(databaseName)}. For example, " +
                $"`mongodb://my-mongo-server` is correct but `mongodb://my-mongo-server/my_database_name` format is incorrect.");
        }

        var pack = customConventionPack ?? new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String),
            new GuidAsStringRepresentationConvention(),
        };

        ConventionRegistry.Register("Custom Conventions", pack, _ => true);

        var mongoSettings = MongoClientSettings.FromUrl(mongoUrl);

        var mongoClient = new MongoClient(mongoSettings);

        return mongoClient.GetDatabase(databaseName);
    }

    private class GuidAsStringRepresentationConvention : ConventionBase, IMemberMapConvention
    {
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberType == typeof(Guid))
            {
                memberMap.SetSerializer(
                    new GuidSerializer(BsonType.String));
            }
            else if (memberMap.MemberType == typeof(Guid?))
            {
                memberMap.SetSerializer(
                    new NullableSerializer<Guid>(new GuidSerializer(BsonType.String)));
            }
        }
    }
}