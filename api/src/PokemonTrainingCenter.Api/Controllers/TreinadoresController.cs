using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Application.DTOs;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Exceptions;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/treinadores")]
public class TreinadoresController : ControllerBase
{
    private readonly ITreinadorRepository _treinadorRepository;

    public TreinadoresController(ITreinadorRepository treinadorRepository)
    {
        _treinadorRepository = treinadorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var treinadores = await _treinadorRepository.ListarAsync();
        return Ok(treinadores.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var treinador = await _treinadorRepository.ObterPorIdAsync(id);
        return treinador is null ? NotFound() : Ok(ToDto(treinador));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTreinadorRequest request)
    {
        var existente = await _treinadorRepository.ObterPorEmailAsync(request.Email);
        if (existente is not null)
        {
            throw new DomainException($"Já existe um Treinador cadastrado com o e-mail {request.Email}.");
        }

        var treinador = new Treinador
        {
            Nome = request.Nome,
            Email = request.Email,
            CidadeOrigem = request.CidadeOrigem,
        };
        await _treinadorRepository.AdicionarAsync(treinador);

        return CreatedAtAction(nameof(ObterPorId), new { id = treinador.Id }, ToDto(treinador));
    }

    private static TreinadorDto ToDto(Treinador treinador) => new(
        treinador.Id,
        treinador.Nome,
        treinador.Email,
        treinador.CidadeOrigem);
}
