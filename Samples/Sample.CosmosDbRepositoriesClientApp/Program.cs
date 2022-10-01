using Firepuma.DatabaseRepositories.CosmosDb;
using Sample.CosmosDbRepositoriesClientApp.Configuration;
using Sample.CosmosDbRepositoriesClientApp.Pets.Entities;
using Sample.CosmosDbRepositoriesClientApp.Pets.Repositories;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCosmosDbRepositories(options =>
{
    options.ConnectionString = configuration.GetValue<string>("CosmosDb:ConnectionString");
    options.DatabaseId = configuration.GetValue<string>("CosmosDb:DatabaseId");
});
builder.Services.AddCosmosDbRepository<
    PetEntity,
    IPetRepository,
    PetCosmosDbRepository>(
    CosmosContainers.Pets.Id,
    (logger, container) => new PetCosmosDbRepository(logger, container));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();