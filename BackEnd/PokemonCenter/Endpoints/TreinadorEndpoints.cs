using PokemonCenter.DTOs;
using PokemonCenter.Services;

namespace PokemonCenter.Endpoints;

public static class TreinadorEndpoints
{
    public static void MapTreinadorEndpoints(this IEndpointRouteBuilder app)
    {
        var treinadores = app.MapGroup("/api/treinadores");

        treinadores.MapGet("/", async (TreinadorService service) =>
        {
            var lista = await service.ListarTodos();
            return Results.Ok(lista);
        });

        treinadores.MapPost("/", async (TreinadorService service, CriarTreinadorRequest request) =>
        {
            var treinador = await service.Criar(request);
            return Results.Created($"/api/treinadores/{treinador.Id}", treinador);
        });

        treinadores.MapGet("/{id:int}", async (int id, TreinadorService service) =>
        {
            var treinador = await service.ObterPorId(id);
            return Results.Ok(treinador);
        });
    }
}
