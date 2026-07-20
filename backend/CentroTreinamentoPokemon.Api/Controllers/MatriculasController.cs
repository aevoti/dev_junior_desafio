using CentroTreinamentoPokemon.Application.Services.Matriculas;
using CentroTreinamentoPokemon.DataTransfer.Requests.Matricula;
using CentroTreinamentoPokemon.DataTransfer.Responses.Matricula;
using CentroTreinamentoPokemon.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CentroTreinamentoPokemon.Api.Controllers;

[ApiController]
[Route("api/matriculas")]
public class MatriculasController : ControllerBase
{
    private readonly IMatriculaService matriculaService;

    public MatriculasController(
        IMatriculaService matriculaService)
    {
        this.matriculaService = matriculaService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] StatusMatriculaEnum? status)
    {
        IEnumerable<MatriculaResponse> response =
            await matriculaService.ListarAsync(
                busca,
                status);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        MatriculaResponse? response =
            await matriculaService.ObterPorIdAsync(id);

        if (response is null)
        {
            return NotFound(
                new
                {
                    mensagem = "Matrícula não encontrada."
                });
        }

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Inserir(
        [FromBody] MatriculaRequest request)
    {
        MatriculaResponse response =
            await matriculaService.CriarAsync(request);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = response.Id },
            response);
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(
        int id,
        [FromBody] CancelarMatriculaRequest request)
    {
        MatriculaResponse? response =
            await matriculaService.CancelarAsync(
                id,
                request);

        if (response is null)
        {
            return NotFound(
                new
                {
                    mensagem = "Matrícula não encontrada."
                });
        }

        return Ok(response);
    }

    [HttpPost("{id}/upgrade/simular")]
    public async Task<IActionResult> SimularUpgrade(
        int id,
        [FromBody] UpgradeMatriculaRequest request)
    {
        UpgradeMatriculaResponse? response =
            await matriculaService.SimularUpgradeAsync(
                id,
                request);

        if (response is null)
        {
            return NotFound(
                new
                {
                    mensagem = "Matrícula não encontrada."
                });
        }

        return Ok(response);
    }

    [HttpPost("{id}/upgrade/confirmar")]
    public async Task<IActionResult> ConfirmarUpgrade(
        int id,
        [FromBody] UpgradeMatriculaRequest request)
    {
        UpgradeMatriculaResponse? response =
            await matriculaService.ConfirmarUpgradeAsync(
                id,
                request);

        if (response is null)
        {
            return NotFound(
                new
                {
                    mensagem = "Matrícula não encontrada."
                });
        }

        return Ok(response);
    }
}