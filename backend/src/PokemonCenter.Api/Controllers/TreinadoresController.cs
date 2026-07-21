using Microsoft.AspNetCore.Mvc;
using PokemonCenter.Application.DTOs;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;

namespace PokemonCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreinadoresController : ControllerBase
{
    private readonly ITreinadorRepository _treinadorRepository;

    public TreinadoresController(ITreinadorRepository treinadorRepository)
    {
        _treinadorRepository = treinadorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var treinadores = await _treinadorRepository.GetAllAsync();
        return Ok(treinadores);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var treinador = await _treinadorRepository.GetByIdAsync(id);
        if (treinador == null) return NotFound();
        return Ok(treinador);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateTreinadorRequest request)
    {
        var existente = await _treinadorRepository.GetByEmailAsync(request.Email);
        if (existente != null)
            return BadRequest(new { mensagem = "Já existe um treinador cadastrado com este e-mail." });

        var treinador = new Treinador
        {
            Nome = request.Nome,
            Email = request.Email,
            CidadeOrigem = request.CidadeOrigem
        };

        var criado = await _treinadorRepository.AddAsync(treinador);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }
}