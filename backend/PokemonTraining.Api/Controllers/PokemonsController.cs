using Microsoft.AspNetCore.Mvc;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PokemonsController(IPokemonService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PokemonResponse>>> Listar(CancellationToken cancellationToken) =>
        Ok(await service.ListarAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> Criar(
        CriarPokemonRequest request,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CriarAsync(request, cancellationToken));

    [HttpPatch("{id:int}/transferencia")]
    public async Task<ActionResult<PokemonResponse>> Transferir(
        int id,
        TransferirPokemonRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.TransferirAsync(id, request, cancellationToken));
}
