using Microsoft.AspNetCore.Mvc;
using PokemonCenter.Application.DTOs;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Domain.Enums;
using PokemonCenter.Domain.Exceptions;

namespace PokemonCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonsController(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pokemons = await _pokemonRepository.GetAllAsync();
        return Ok(pokemons.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pokemon = await _pokemonRepository.GetByIdAsync(id);
        if (pokemon == null) return NotFound();
        return Ok(MapToResponse(pokemon));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreatePokemonRequest request)
    {
        if (!Enum.TryParse<TipoPokemon>(request.Tipo, ignoreCase: true, out var tipo))
            throw new DomainException($"Tipo de Pokémon inválido: '{request.Tipo}'.");

        var pokemon = new Pokemon
        {
            Nome = request.Nome,
            Tipo = tipo,
            Nivel = request.Nivel,
            TreinadorId = request.TreinadorId
        };

        var criado = await _pokemonRepository.AddAsync(pokemon);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, MapToResponse(criado));
    }

    [HttpPatch("{id}/transferir")]
    public async Task<IActionResult> Transferir(int id, [FromBody] TransferirPokemonRequest request)
    {
        var pokemon = await _pokemonRepository.GetByIdAsync(id);
        if (pokemon == null) return NotFound();

        pokemon.TreinadorId = request.NovoTreinadorId;
        await _pokemonRepository.UpdateAsync(pokemon);

        return NoContent();
    }

    private static PokemonResponse MapToResponse(Pokemon p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Tipo = p.Tipo.ToString(),
        Nivel = p.Nivel,
        TreinadorId = p.TreinadorId,
        TreinadorNome = p.Treinador?.Nome
    };
}

public class TransferirPokemonRequest
{
    public int NovoTreinadorId { get; set; }
}