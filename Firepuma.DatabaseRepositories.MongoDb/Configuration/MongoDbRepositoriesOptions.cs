using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618

namespace Firepuma.DatabaseRepositories.MongoDb.Configuration;

public class MongoDbRepositoriesOptions
{
    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public string DatabaseName { get; set; }
}