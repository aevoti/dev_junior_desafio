using CentroTreinamentoPokemon.Application.Services.PlanosTreinamento;
using CentroTreinamentoPokemon.DataTransfer.Responses.PlanoTreinamento;
using Microsoft.AspNetCore.Mvc;

namespace CentroTreinamentoPokemon.Api.Controllers;

[ApiController]
[Route("api/planos")]
public class PlanosTreinamentoController : ControllerBase
{
    private readonly IPlanoTreinamentoService planoTreinamentoService;

    public PlanosTreinamentoController(
        IPlanoTreinamentoService planoTreinamentoService)
    {
        this.planoTreinamentoService = planoTreinamentoService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        IEnumerable<PlanoTreinamentoResponse> response =
            await planoTreinamentoService.ListarAsync();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        PlanoTreinamentoResponse? response =
            await planoTreinamentoService.ObterPorIdAsync(id);

        if (response is null)
            return NotFound();

        return Ok(response);
    }
}