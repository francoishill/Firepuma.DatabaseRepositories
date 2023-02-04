namespace Firepuma.DatabaseRepositories.Abstractions.Exceptions;

[Serializable]
public class DatabaseItemNotFoundException : Exception
{
    public DatabaseItemNotFoundException(Type itemType, string id)
        : base($"Item of type {itemType} and id '{id}' not found")
    {
    }
}