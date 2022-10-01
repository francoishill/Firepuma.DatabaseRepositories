using Firepuma.DatabaseRepositories.CosmosDb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Sample.CosmosDbRepositoriesClientApp.Configuration;

namespace Sample.CosmosDbRepositoriesClientApp.Admin.Controllers;

[ApiController]
[Route("[controller]")]
//TODO: ensure to authorize only Admin users to use these endpoints
public class AdminController : ControllerBase
{
    private readonly ICosmosDbAdminService _cosmosDbAdminService;

    public AdminController(
        ICosmosDbAdminService cosmosDbAdminService)
    {
        _cosmosDbAdminService = cosmosDbAdminService;
    }

    [HttpPut("create-cosmos-containers")]
    public async Task<IActionResult> CreateCosmosContainers(CancellationToken cancellationToken)
    {
        var containersToCreate = new[]
        {
            CosmosContainers.Pets,
        };

        var result = await _cosmosDbAdminService.CreateContainersIfNotExist(containersToCreate, cancellationToken);

        //TODO: map result to Api DTO object

        return Ok(result);
    }
}