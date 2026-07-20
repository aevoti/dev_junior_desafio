using PokemonCenter.Services;

namespace PokemonCenter.Endpoints;

public static class PlanoEndpoints
{
    public static void MapPlanoEndpoints(this IEndpointRouteBuilder app)
    {
        var planos = app.MapGroup("/api/planos");

        planos.MapGet("/", async (PlanoService service) =>
        {
            var lista = await service.ListarTodos();
            return Results.Ok(lista);
        });
    }
}
