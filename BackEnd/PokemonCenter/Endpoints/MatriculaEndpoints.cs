using PokemonCenter.DTOs;
using PokemonCenter.Services;

namespace PokemonCenter.Endpoints;

public static class MatriculaEndpoints
{
    public static void MapMatriculaEndpoints(this IEndpointRouteBuilder app)
    {
        var matriculas = app.MapGroup("api/matriculas");

        matriculas.MapGet("/", async (MatriculaService service) =>
        {
            var lista = await service.ListarTodos();
            return TypedResults.Ok(lista);
        });

        matriculas.MapGet("/{id:int}", async (MatriculaService service, int id) =>
        {
            var matricula = await service.ObterPorId(id);
            return TypedResults.Ok(matricula);
        });

        matriculas.MapPost("/", async (MatriculaService service, CriarMatriculaRequest request) =>
        {
            var novaMatricula = await service.Criar(request);
            return TypedResults.Created($"/api/matriculas/{novaMatricula.Id}", novaMatricula);
        });

        matriculas.MapPatch("/{id:int}/cancelar", async (MatriculaService service, int id) =>
        {
            var matricula = await service.Cancelar(id);
            return TypedResults.Ok(matricula);
        });

        matriculas.MapGet("/{matriculaId:int}/upgrade", async (MatriculaService service, int matriculaId, int novoPlanoId) =>
        {
            var simulacao = await service.SimularUpgrade(matriculaId, novoPlanoId);
            return TypedResults.Ok(simulacao);
        });

        matriculas.MapPatch("/{matriculaId:int}/upgrade", async (MatriculaService service, int matriculaId, UpgradeMatriculaRequest request) =>
        {
            var matricula = await service.Upgrade(matriculaId, request.NovoPlanoId);
            return TypedResults.Ok(matricula);
        });
    }
}
