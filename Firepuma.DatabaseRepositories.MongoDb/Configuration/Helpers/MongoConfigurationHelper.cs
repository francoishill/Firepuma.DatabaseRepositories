using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Firepuma.DatabaseRepositories.MongoDb.Configuration.Helpers
{
    public static class MongoConfigurationHelper
    {
        public static IMongoDatabase ConfigureMongoDatabase(MongoUrl mongoUrl)
        {
            var pack = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String),
                new GuidAsStringRepresentationConvention(),
            };

            ConventionRegistry.Register("Custom Conventions", pack, _ => true);

            var mongoSettings = MongoClientSettings.FromUrl(mongoUrl);

            var mongoClient = new MongoClient(mongoSettings);

            return mongoClient.GetDatabase(mongoUrl.DatabaseName);
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
}