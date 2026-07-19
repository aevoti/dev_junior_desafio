using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Application.Interfaces;

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
        return Ok(treinadores);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var treinador = await _treinadorRepository.ObterPorIdAsync(id);
        return treinador is null ? NotFound() : Ok(treinador);
    }
}
