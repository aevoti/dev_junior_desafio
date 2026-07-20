using Microsoft.AspNetCore.Mvc;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanosController(IPlanoTreinamentoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlanoTreinamentoResponse>>> Listar(CancellationToken cancellationToken) =>
        Ok(await service.ListarAsync(cancellationToken));
}
