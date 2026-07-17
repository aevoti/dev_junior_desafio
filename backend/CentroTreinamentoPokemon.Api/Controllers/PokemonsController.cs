using CentroTreinamentoPokemon.Application.Services.Pokemons;
using CentroTreinamentoPokemon.DataTransfer.Requests.Pokemon;
using CentroTreinamentoPokemon.DataTransfer.Responses.Pokemon;
using Microsoft.AspNetCore.Mvc;

namespace CentroTreinamentoPokemon.Api.Controllers;

[ApiController]
[Route("api/pokemons")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonService pokemonService;

    public PokemonsController(
        IPokemonService pokemonService)
    {
        this.pokemonService = pokemonService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        IEnumerable<PokemonResponse> response =
            await pokemonService.ListarAsync();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        PokemonResponse? response =
            await pokemonService.ObterPorIdAsync(id);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpGet("treinador/{treinadorId}")]
    public async Task<IActionResult> ListarPorTreinador(
        int treinadorId)
    {
        IEnumerable<PokemonResponse> response =
            await pokemonService.ListarPorTreinadorAsync(
                treinadorId);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Inserir(
        [FromBody] PokemonRequest request)
    {
        PokemonResponse response =
            await pokemonService.CriarAsync(request);

        return Ok(response);
    }

    [HttpPost("{id}/transferir")]
    public async Task<IActionResult> Transferir(
        int id,
        [FromBody] TransferirPokemonRequest request)
    {
        PokemonResponse? response =
            await pokemonService.TransferirAsync(
                id,
                request);

        if (response is null)
            return NotFound();

        return Ok(response);
    }
}