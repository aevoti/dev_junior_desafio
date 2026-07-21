using Microsoft.AspNetCore.Mvc;
using PokemonCenter.Application.Interfaces;

namespace PokemonCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanosController : ControllerBase
{
    private readonly IPlanoRepository _planoRepository;

    public PlanosController(IPlanoRepository planoRepository)
    {
        _planoRepository = planoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var planos = await _planoRepository.GetAllAsync();
        return Ok(planos);
    }
}