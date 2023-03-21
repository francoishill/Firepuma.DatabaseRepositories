namespace Firepuma.DatabaseRepositories.Abstractions.Repositories.Exceptions;

public class DuplicateDatabaseEntityException : Exception
{
    public DuplicateDatabaseEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}