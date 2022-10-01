# Firepuma.DatabaseRepositories

A project containing database repositories (repository pattern).

## Getting started

See the example project in `Samples/Sample.CosmosDbRepositoriesClientApp`, it uses:

* `builder.Services.AddCosmosDbRepository` to add `PetCosmosDbRepository` (extends `CosmosDbRepository`)
* `cosmosDbAdminService.CreateContainersIfNotExist`
* `QuerySpecification`


## Credits

* https://medium.com/swlh/clean-architecture-with-partitioned-repository-pattern-using-azure-cosmos-db-62241854cbc5