using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Application.Interfaces;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/planos-treinamento")]
public class PlanosTreinamentoController : ControllerBase
{
    private readonly IPlanoTreinamentoRepository _planoRepository;

    public PlanosTreinamentoController(IPlanoTreinamentoRepository planoRepository)
    {
        _planoRepository = planoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var planos = await _planoRepository.ListarAsync();
        return Ok(planos);
    }
}
