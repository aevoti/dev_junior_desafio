using CentroTreinamentoPokemon.Application.Services.Treinadores;
using CentroTreinamentoPokemon.DataTransfer.Requests.Treinador;
using CentroTreinamentoPokemon.DataTransfer.Responses.Treinador;
using Microsoft.AspNetCore.Mvc;

namespace CentroTreinamentoPokemon.Api.Controllers;

[ApiController]
[Route("api/treinadores")]
public class TreinadoresController : ControllerBase
{
    private readonly ITreinadorService treinadorService;

    public TreinadoresController(
        ITreinadorService treinadorService)
    {
        this.treinadorService = treinadorService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        IEnumerable<TreinadorResponse> response =
            await treinadorService.ListarAsync();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        TreinadorResponse? response =
            await treinadorService.ObterPorIdAsync(id);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Inserir(
        [FromBody] TreinadorRequest request)
    {
        TreinadorResponse response =
            await treinadorService.CriarAsync(request);

        return Ok(response);
    }
}