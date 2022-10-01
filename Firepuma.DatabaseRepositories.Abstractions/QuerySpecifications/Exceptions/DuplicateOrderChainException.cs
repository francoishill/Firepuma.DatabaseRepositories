namespace Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications.Exceptions;

public class DuplicateOrderChainException : Exception
{
    private const string MESSAGE = "The query contains more than one Order chain!";

    public DuplicateOrderChainException()
        : base(MESSAGE)
    {
    }

    public DuplicateOrderChainException(Exception innerException)
        : base(MESSAGE, innerException)
    {
    }
}