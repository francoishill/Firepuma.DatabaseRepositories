using Microsoft.AspNetCore.Mvc;
using Sample.MongoDbRepositoriesClientApp.Pets.Controllers.Requests;
using Sample.MongoDbRepositoriesClientApp.Pets.Entities;
using Sample.MongoDbRepositoriesClientApp.Pets.QuerySpecifications;
using Sample.MongoDbRepositoriesClientApp.Pets.Repositories;

namespace Sample.MongoDbRepositoriesClientApp.Pets.Controllers;

[ApiController]
[Route("[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetRepository _petRepository;

    public PetsController(
        IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    [HttpPost]
    public async Task<PetEntity> AddPet(AddPetRequest request, CancellationToken cancellationToken)
    {
        var newPet = new PetEntity
        {
            Id = _petRepository.GenerateId(),
            Type = request.Type,
            Name = request.Name,
            BornOn = request.BornOn ?? throw new ArgumentNullException($"{nameof(request.BornOn)} is required"),
            ArrivedOn = request.ArrivedOn ?? throw new ArgumentNullException($"{nameof(request.ArrivedOn)} is required"),
        };
        newPet = await _petRepository.AddItemAsync(newPet, cancellationToken);

        return newPet;
    }

    [HttpGet]
    public async Task<IEnumerable<PetEntity>> GetAllPets(CancellationToken cancellationToken)
    {
        var querySpecification = new AllPetsQuerySpecification();
        var pets = await _petRepository.GetItemsAsync(querySpecification, cancellationToken);
        //TODO: map result to Api DTO objects
        return pets;
    }

    [HttpGet("recently-arrived")]
    public async Task<IEnumerable<PetEntity>> GetAllRecentlyArrivedPets(CancellationToken cancellationToken)
    {
        var arrivedAfterDate = DateTime.UtcNow.AddMonths(-1);
        var querySpecification = new RecentlyArrivedPetsQuerySpecification(arrivedAfterDate);
        var pets = await _petRepository.GetItemsAsync(querySpecification, cancellationToken);
        //TODO: map result to Api DTO objects
        return pets;
    }
}