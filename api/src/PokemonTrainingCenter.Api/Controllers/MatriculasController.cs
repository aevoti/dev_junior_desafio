using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Application.DTOs;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/matriculas")]
public class MatriculasController : ControllerBase
{
    private readonly IMatriculaService _matriculaService;

    public MatriculasController(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
    }

    /// <summary>Lista matrículas com busca por nome do Pokémon/Treinador e filtro por status.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busca, [FromQuery] StatusMatricula? status)
    {
        var matriculas = await _matriculaService.ListarAsync(busca, status);
        return Ok(matriculas);
    }

    /// <summary>Cria uma nova matrícula. Rejeita conforme R1 (matrícula ativa duplicada) e R3 (nível mínimo).</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarMatriculaRequest request)
    {
        var matricula = await _matriculaService.CriarAsync(request);
        return CreatedAtAction(nameof(Listar), new { }, matricula);
    }

    /// <summary>Executa o upgrade de plano (R2) e retorna o valor pro-rata da primeira cobrança.</summary>
    [HttpPost("{id:int}/upgrade")]
    public async Task<IActionResult> Upgrade(int id, [FromBody] UpgradeMatriculaRequest request)
    {
        var resultado = await _matriculaService.UpgradeAsync(id, request);
        return Ok(resultado);
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _matriculaService.CancelarAsync(id);
        return NoContent();
    }
}
