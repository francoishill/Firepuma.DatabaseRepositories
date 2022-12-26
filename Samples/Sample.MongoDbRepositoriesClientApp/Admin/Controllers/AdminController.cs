using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Indexes;
using Microsoft.AspNetCore.Mvc;

namespace Sample.MongoDbRepositoriesClientApp.Admin.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    private readonly IMongoIndexesApplier _mongoIndexesApplier;

    public AdminController(
        IMongoIndexesApplier mongoIndexesApplier)
    {
        _mongoIndexesApplier = mongoIndexesApplier;
    }

    [HttpPost("apply-database-indexes")]
    public async Task ApplyDatabaseIndexes(CancellationToken cancellationToken)
    {
        await _mongoIndexesApplier.ApplyAllIndexes(cancellationToken);
    }
}