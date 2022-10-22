using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618

namespace Firepuma.DatabaseRepositories.CosmosDb.Configuration;

public class CosmosDbRepositoriesOptions
{
    [Required]
    public string ConnectionString { get; set; }

    [Required]
    public string DatabaseId { get; set; }
}