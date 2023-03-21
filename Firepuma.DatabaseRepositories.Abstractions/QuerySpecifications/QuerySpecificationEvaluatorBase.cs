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
            IOrderedQueryable<T>? orderedQuery = null;
            foreach (var orderExpression in querySpecification.OrderExpressions)
            {
                if (orderedQuery == null)
                {
                    orderedQuery = orderExpression.OrderType == OrderTypeEnum.Ascending
                        ? queryable.OrderBy(orderExpression.KeySelector)
                        : queryable.OrderByDescending(orderExpression.KeySelector);
                }
                else
                {
                    orderedQuery = orderExpression.OrderType == OrderTypeEnum.Ascending
                        ? orderedQuery.ThenBy(orderExpression.KeySelector)
                        : orderedQuery.ThenByDescending(orderExpression.KeySelector);
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