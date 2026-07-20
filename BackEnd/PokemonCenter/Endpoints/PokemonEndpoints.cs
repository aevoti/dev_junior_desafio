using PokemonCenter.DTOs;
using PokemonCenter.Services;

namespace PokemonCenter.Endpoints;

public static class PokemonEndpoints
{
    public static void MapPokemonEndpoints(this IEndpointRouteBuilder app)
    {
        var pokemons = app.MapGroup("/api/pokemons");

        pokemons.MapGet("/", async (PokemonService service) =>
        {
            var lista = await service.ListarTodos();
            return Results.Ok(lista);
        });

        pokemons.MapGet("/{id:int}", async (int id, PokemonService service) =>
        {
            var pokemon = await service.ObterPorId(id);
            return Results.Ok(pokemon);
        });

        pokemons.MapPost("/", async (PokemonService service, CriarPokemonRequest request) =>
        {
            var pokemon = await service.Criar(request);
            return Results.Created($"/api/pokemons/{pokemon.Id}", pokemon);
        });

        pokemons.MapPatch("/{id:int}/transferir", async (int id, TransferirPokemonRequest request, PokemonService service) =>
        {
            var pokemon = await service.Transferir(id, request.NovoTreinadorId);
            return Results.Ok(pokemon);
        });
    }
}
