using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;

namespace Firepuma.DatabaseRepositories.CosmosDb.Queries;

public class CosmosDbQuerySpecificationEvaluator<T> : QuerySpecificationEvaluatorBase<T> where T : class
{
}