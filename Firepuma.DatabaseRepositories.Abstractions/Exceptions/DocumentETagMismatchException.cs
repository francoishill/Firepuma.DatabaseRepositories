namespace Firepuma.DatabaseRepositories.Abstractions.Exceptions;

[Serializable]
public class DocumentETagMismatchException : Exception
{
    public DocumentETagMismatchException()
    {
    }

    public DocumentETagMismatchException(string message)
        : base(message)
    {
    }

    public DocumentETagMismatchException(string message, Exception inner)
        : base(message, inner)
    {
    }
}