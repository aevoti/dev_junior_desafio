using Microsoft.AspNetCore.Mvc;
using PokemonCenter.Application.DTOs;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Domain.Enums;
using PokemonCenter.Domain.Exceptions;

namespace PokemonCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatriculasController : ControllerBase
{
    private readonly IMatriculaService _matriculaService;
    private readonly IMatriculaRepository _matriculaRepository;

    public MatriculasController(IMatriculaService matriculaService, IMatriculaRepository matriculaRepository)
    {
        _matriculaService = matriculaService;
        _matriculaRepository = matriculaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? busca, [FromQuery] StatusMatricula? status)
    {
        var matriculas = await _matriculaRepository.GetAllAsync(busca, status);
        return Ok(matriculas.Select(MapToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateMatriculaRequest request)
    {
        var matricula = await _matriculaService.CriarMatriculaAsync(request);

        var matriculaCompleta = await _matriculaRepository.GetByIdAsync(matricula.Id)
            ?? throw new DomainException("Erro ao carregar a matrícula recém-criada.");

        return CreatedAtAction(nameof(GetAll), new { id = matriculaCompleta.Id }, MapToResponse(matriculaCompleta));
    }

    [HttpPost("{id}/upgrade/simular")]
    public async Task<IActionResult> SimularUpgrade(int id, [FromBody] UpgradeRequest request)
    {
        var resultado = await _matriculaService.SimularUpgradeAsync(id, request.NovoPlanoId, request.DataUpgrade);
        return Ok(resultado);
    }

    [HttpPost("{id}/upgrade/confirmar")]
    public async Task<IActionResult> ConfirmarUpgrade(int id, [FromBody] UpgradeRequest request)
    {
        var novaMatricula = await _matriculaService.ConfirmarUpgradeAsync(id, request.NovoPlanoId, request.DataUpgrade);

        var matriculaCompleta = await _matriculaRepository.GetByIdAsync(novaMatricula.Id)
            ?? throw new DomainException("Erro ao carregar a matrícula após o upgrade.");

        return Ok(MapToResponse(matriculaCompleta));
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _matriculaService.CancelarMatriculaAsync(id);
        return NoContent();
    }

    private static MatriculaResponse MapToResponse(Matricula m) => new()
    {
        Id = m.Id,
        PokemonNome = m.Pokemon!.Nome,
        TreinadorNome = m.Pokemon.Treinador!.Nome,
        PlanoNome = m.Plano!.Nome,
        ValorMensal = m.ValorMensal,
        DataInicio = m.DataInicio,
        Status = m.Status.ToString()
    };
}