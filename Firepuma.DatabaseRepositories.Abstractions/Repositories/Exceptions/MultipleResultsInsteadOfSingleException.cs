namespace Firepuma.DatabaseRepositories.Abstractions.Repositories.Exceptions;

public class MultipleResultsInsteadOfSingleException : Exception
{
    public MultipleResultsInsteadOfSingleException(string message)
        : base(message)
    {
    }
}