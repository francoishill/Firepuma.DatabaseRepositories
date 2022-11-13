using Firepuma.DatabaseRepositories.MongoDb;
using Sample.MongoDbRepositoriesClientApp.Configuration;
using Sample.MongoDbRepositoriesClientApp.Pets.Entities;
using Sample.MongoDbRepositoriesClientApp.Pets.Repositories;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMongoDbRepositories(options =>
{
    options.ConnectionString = configuration.GetValue<string>("MongoDb:ConnectionString");
    options.DatabaseName = configuration.GetValue<string>("MongoDb:DatabaseName");
});
builder.Services.AddMongoDbRepository<
    PetEntity,
    IPetRepository,
    PetMongoDbRepository>(
    MongoCollectionNames.Pets,
    (logger, container, _) => new PetMongoDbRepository(logger, container));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();