namespace Firepuma.DatabaseRepositories.Abstractions.Repositories.Exceptions;

public class MultipleResultsInsteadOfSingleException : Exception
{
    public MultipleResultsInsteadOfSingleException()
    {
    }

    public MultipleResultsInsteadOfSingleException(string message)
        : base(message)
    {
    }

    public MultipleResultsInsteadOfSingleException(string message, Exception inner)
        : base(message, inner)
    {
    }
}