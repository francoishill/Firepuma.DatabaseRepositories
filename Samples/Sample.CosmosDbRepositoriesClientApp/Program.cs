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

var connectionString = configuration.GetValue<string>("CosmosDb:ConnectionString") ?? throw new Exception("CosmosDb:ConnectionString config is required");
var databaseId = configuration.GetValue<string>("CosmosDb:DatabaseId") ?? throw new Exception("CosmosDb:DatabaseId config is required");
builder.Services.AddCosmosDbRepositories(options =>
{
    options.ConnectionString = connectionString;
    options.DatabaseId = databaseId;
});
builder.Services.AddCosmosDbRepository<
    PetEntity,
    IPetRepository,
    PetCosmosDbRepository>(
    CosmosContainers.Pets.ContainerProperties.Id,
    (logger, container, _) => new PetCosmosDbRepository(logger, container));

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