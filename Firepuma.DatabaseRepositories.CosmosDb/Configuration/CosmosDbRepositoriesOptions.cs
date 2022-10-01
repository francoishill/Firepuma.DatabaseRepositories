using System.ComponentModel.DataAnnotations;

namespace Firepuma.DatabaseRepositories.CosmosDb.Configuration;

public class CosmosDbRepositoriesOptions
{
    [Required]
    public string ConnectionString { get; set; }

    [Required]
    public string DatabaseId { get; set; }
}