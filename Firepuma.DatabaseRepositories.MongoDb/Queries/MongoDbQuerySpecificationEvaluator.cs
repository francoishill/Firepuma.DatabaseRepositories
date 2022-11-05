using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;

namespace Firepuma.DatabaseRepositories.MongoDb.Queries;

public class MongoDbQuerySpecificationEvaluator<T> : QuerySpecificationEvaluatorBase<T> where T : class
{
}