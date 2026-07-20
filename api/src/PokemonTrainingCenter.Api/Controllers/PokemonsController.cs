using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Application.DTOs;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Exceptions;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/pokemons")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonRepository _pokemonRepository;
    private readonly ITreinadorRepository _treinadorRepository;

    public PokemonsController(IPokemonRepository pokemonRepository, ITreinadorRepository treinadorRepository)
    {
        _pokemonRepository = pokemonRepository;
        _treinadorRepository = treinadorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var pokemons = await _pokemonRepository.ListarAsync();
        return Ok(pokemons.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var pokemon = await _pokemonRepository.ObterPorIdAsync(id);
        return pokemon is null ? NotFound() : Ok(ToDto(pokemon));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPokemonRequest request)
    {
        var treinador = await _treinadorRepository.ObterPorIdAsync(request.TreinadorId)
            ?? throw new DomainException("Treinador não encontrado.");

        if (request.Nivel is < 1 or > 100)
        {
            throw new DomainException("O nível do Pokémon deve estar entre 1 e 100.");
        }

        var pokemon = new Pokemon
        {
            Nome = request.Nome,
            Tipo = request.Tipo,
            Nivel = request.Nivel,
            TreinadorId = treinador.Id,
        };
        await _pokemonRepository.AdicionarAsync(pokemon);

        return CreatedAtAction(nameof(ObterPorId), new { id = pokemon.Id }, ToDto(pokemon) with { TreinadorNome = treinador.Nome });
    }

    /// <summary>R5: transfere o Pokémon para outro Treinador. Como a Matrícula referencia o
    /// Pokémon (não o Treinador) diretamente, matrículas ativas e históricas acompanham o
    /// Pokémon automaticamente — não é necessário duplicar ou recriar nada.</summary>
    [HttpPut("{id:int}/transferir")]
    public async Task<IActionResult> Transferir(int id, [FromBody] TransferirPokemonRequest request)
    {
        var pokemon = await _pokemonRepository.ObterPorIdAsync(id)
            ?? throw new DomainException("Pokémon não encontrado.");

        var novoTreinador = await _treinadorRepository.ObterPorIdAsync(request.NovoTreinadorId)
            ?? throw new DomainException("Treinador de destino não encontrado.");

        if (novoTreinador.Id == pokemon.TreinadorId)
        {
            throw new DomainException($"{pokemon.Nome} já pertence a {novoTreinador.Nome}.");
        }

        pokemon.TreinadorId = novoTreinador.Id;
        await _pokemonRepository.AtualizarAsync(pokemon);

        return Ok(ToDto(pokemon) with { TreinadorId = novoTreinador.Id, TreinadorNome = novoTreinador.Nome });
    }

    private static PokemonDto ToDto(Pokemon pokemon) => new(
        pokemon.Id,
        pokemon.Nome,
        pokemon.Tipo,
        pokemon.Nivel,
        pokemon.TreinadorId,
        pokemon.Treinador?.Nome ?? string.Empty);
}
