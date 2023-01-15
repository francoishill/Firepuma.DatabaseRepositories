using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications.Exceptions;

namespace Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;

public abstract class QuerySpecificationEvaluatorBase<T> : IQuerySpecificationEvaluator<T> where T : class
{
    public virtual IQueryable<TResult> GetQuery<TResult>(IQueryable<T> inputQuery, IQuerySpecification<T, TResult> querySpecification)
    {
        var queryable = GetQuery(inputQuery, (IQuerySpecification<T>)querySpecification);

        // Apply selector
        var selectQuery = queryable.Select(querySpecification.Selector);

        return selectQuery;
    }

    public virtual IQueryable<T> GetQuery(IQueryable<T> inputQuery, IQuerySpecification<T> querySpecification)
    {
        var queryable = querySpecification
            .WhereExpressions
            .Aggregate(inputQuery, (current, criteria) => current.Where(criteria));

        if (querySpecification.OrderExpressions.Any())
        {
            if (querySpecification.OrderExpressions.Count(x => x.OrderType is OrderTypeEnum.OrderBy or OrderTypeEnum.OrderByDescending) > 1)
            {
                throw new DuplicateOrderChainException();
            }

            IOrderedQueryable<T>? orderedQuery = null;
            foreach (var orderExpression in querySpecification.OrderExpressions)
            {
                if (orderExpression.OrderType == OrderTypeEnum.OrderBy)
                {
                    orderedQuery = queryable.OrderBy(orderExpression.KeySelector);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.OrderByDescending)
                {
                    orderedQuery = queryable.OrderByDescending(orderExpression.KeySelector);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.ThenBy)
                {
                    orderedQuery = orderedQuery?.ThenBy(orderExpression.KeySelector);
                }
                else if (orderExpression.OrderType == OrderTypeEnum.ThenByDescending)
                {
                    orderedQuery = orderedQuery?.ThenByDescending(orderExpression.KeySelector);
                }
            }

            if (orderedQuery != null)
            {
                queryable = orderedQuery;
            }
        }

        // If skip is 0, avoid adding to the IQueryable. It will generate more optimized SQL that way.
        if (querySpecification.Skip != null && querySpecification.Skip != 0)
        {
            queryable = queryable.Skip(querySpecification.Skip.Value);
        }

        if (querySpecification.Take != null)
        {
            queryable = queryable.Take(querySpecification.Take.Value);
        }

        return queryable;
    }
}