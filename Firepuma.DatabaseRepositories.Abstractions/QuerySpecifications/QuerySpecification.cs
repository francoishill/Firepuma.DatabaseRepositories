using System.Linq.Expressions;

namespace Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;

public class QuerySpecification<T> : IQuerySpecification<T>
{
    public List<Expression<Func<T, bool>>> WhereExpressions { get; } = new();

    public List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)> OrderExpressions { get; } = new();

    public int? Take { get; set; } = null;

    public int? Skip { get; set; } = null;
}