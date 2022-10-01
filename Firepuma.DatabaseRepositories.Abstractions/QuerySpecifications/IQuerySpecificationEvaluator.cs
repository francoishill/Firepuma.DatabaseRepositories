namespace Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;

public interface IQuerySpecificationEvaluator<T> where T : class
{
    // https://github.com/ardalis/Specification/blob/2a2aecc26fd1930fdcfaebcaafc36873358d5456/ArdalisSpecification/src/Ardalis.Specification/ISpecificationEvaluator.cs

    IQueryable<TResult> GetQuery<TResult>(IQueryable<T> inputQuery, IQuerySpecification<T, TResult> querySpecification);
    IQueryable<T> GetQuery(IQueryable<T> inputQuery, IQuerySpecification<T> querySpecification);
}