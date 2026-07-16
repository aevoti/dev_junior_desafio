using Microsoft.AspNetCore.Mvc;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreinadoresController(ITreinadorService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TreinadorResponse>>> Listar(CancellationToken cancellationToken) =>
        Ok(await service.ListarAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<TreinadorResponse>> Criar(
        CriarTreinadorRequest request,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CriarAsync(request, cancellationToken));
}
