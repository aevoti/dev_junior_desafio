using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Api.Contracts;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;
using PokemonTrainingCenter.Domain.Repositories;
using PokemonTrainingCenter.Domain.Services;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/pokemons")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonRepository _pokemons;
    private readonly ITrainerRepository _trainers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EnrollmentService _enrollmentService;

    public PokemonsController(
        IPokemonRepository pokemons, ITrainerRepository trainers, IUnitOfWork unitOfWork, EnrollmentService enrollmentService)
    {
        _pokemons = pokemons;
        _trainers = trainers;
        _unitOfWork = unitOfWork;
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> Create(CreatePokemonRequest request)
    {
        // FR-004: nível deve estar entre 1 e 100.
        if (request.Level is < 1 or > 100)
        {
            return BadRequest(new { message = "O nível do Pokémon deve estar entre 1 e 100." });
        }

        // FR-022: tipo deve ser um dos 18 valores fixos.
        if (!Enum.TryParse<PokemonType>(request.Type, ignoreCase: false, out var type))
        {
            return BadRequest(new { message = "Tipo de Pokémon inválido." });
        }

        var trainer = await _trainers.GetByIdAsync(request.TrainerId);
        if (trainer is null)
        {
            return NotFound(new { message = "Treinador não encontrado." });
        }

        var pokemon = new Pokemon
        {
            Name = request.Name,
            Type = type,
            Level = request.Level,
            TrainerId = request.TrainerId,
            Trainer = trainer
        };

        _pokemons.Add(pokemon);
        await _unitOfWork.SaveChangesAsync();

        var response = ToResponse(pokemon);
        return CreatedAtAction(nameof(GetAll), new { id = pokemon.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PokemonResponse>>> GetAll()
    {
        var pokemons = await _pokemons.GetAllWithTrainerAsync();

        return Ok(pokemons.Select(ToResponse));
    }

    [HttpPost("{id:int}/transfer")]
    public async Task<ActionResult<TransferPokemonResponse>> Transfer(int id, TransferPokemonRequest request)
    {
        var (pokemon, closed, created) = await _enrollmentService.TransferPokemonAsync(id, request.NewTrainerId);
        var response = new TransferPokemonResponse(ToResponse(pokemon), closed?.Id, created?.Id);
        return Ok(response);
    }

    private static PokemonResponse ToResponse(Pokemon pokemon) =>
        new(pokemon.Id, pokemon.Name, pokemon.Type.ToString(), pokemon.Level, pokemon.TrainerId, pokemon.Trainer!.Name);
}
