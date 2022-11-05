using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618

namespace Firepuma.DatabaseRepositories.MongoDb.Configuration;

public class MongoDbRepositoriesOptions
{
    [Required]
    public string Url { get; set; } = null!;
}