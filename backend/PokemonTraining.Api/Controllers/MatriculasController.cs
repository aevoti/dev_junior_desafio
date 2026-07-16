using Microsoft.AspNetCore.Mvc;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Enums;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatriculasController(IMatriculaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MatriculaResponse>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] StatusMatricula? status,
        CancellationToken cancellationToken) =>
        Ok(await service.ListarAsync(busca, status, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<MatriculaResponse>> Criar(
        CriarMatriculaRequest request,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CriarAsync(request, cancellationToken));

    [HttpPatch("{id:int}/cancelamento")]
    public async Task<ActionResult<MatriculaResponse>> Cancelar(
        int id,
        CancelarMatriculaRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.CancelarAsync(id, request, cancellationToken));

    [HttpPost("{id:int}/simular-upgrade")]
    public async Task<ActionResult<SimulacaoUpgradeResponse>> SimularUpgrade(
        int id,
        UpgradeMatriculaRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.SimularUpgradeAsync(id, request, cancellationToken));

    [HttpPost("{id:int}/upgrade")]
    public async Task<ActionResult<UpgradeMatriculaResponse>> RealizarUpgrade(
        int id,
        UpgradeMatriculaRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.RealizarUpgradeAsync(id, request, cancellationToken));
}
