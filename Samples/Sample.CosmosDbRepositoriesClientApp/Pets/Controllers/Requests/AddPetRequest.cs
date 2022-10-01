using System.ComponentModel.DataAnnotations;

namespace Sample.CosmosDbRepositoriesClientApp.Pets.Controllers.Requests;

public class AddPetRequest
{
    [Required]
    public string Type { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public DateTime? BornOn { get; set; }

    [Required]
    public DateTime? ArrivedOn { get; set; }
}