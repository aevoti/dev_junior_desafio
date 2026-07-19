using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Application.Interfaces;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/pokemons")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonsController(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var pokemons = await _pokemonRepository.ListarAsync();
        return Ok(pokemons);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var pokemon = await _pokemonRepository.ObterPorIdAsync(id);
        return pokemon is null ? NotFound() : Ok(pokemon);
    }

    // TODO R5: endpoint de transferência de Pokémon para outro Treinador,
    // definindo o comportamento das matrículas existentes (ver README > Premissas).
}
