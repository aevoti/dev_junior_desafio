using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Data;
using PokemonTrainingCenter.DTOs;
using PokemonTrainingCenter.Models;

namespace PokemonTrainingCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreinadoresController : ControllerBase
{
    private readonly AppDbContext _db;

    public TreinadoresController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<TreinadorResponse>> GetAll()
    {
        return await _db.Treinadores
            .Select(t => new TreinadorResponse(t.Id, t.Nome, t.Email, t.CidadeOrigem))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TreinadorResponse>> GetById(int id)
    {
        var t = await _db.Treinadores.FindAsync(id);
        if (t is null) return NotFound();
        return new TreinadorResponse(t.Id, t.Nome, t.Email, t.CidadeOrigem);
    }

    [HttpPost]
    public async Task<ActionResult<TreinadorResponse>> Create([FromBody] TreinadorRequest req)
    {
        if (await _db.Treinadores.AnyAsync(t => t.Email == req.Email))
            return Conflict(new { erro = "Já existe um Treinador cadastrado com este e-mail." });

        var treinador = new Treinador
        {
            Nome = req.Nome,
            Email = req.Email,
            CidadeOrigem = req.CidadeOrigem
        };
        _db.Treinadores.Add(treinador);
        await _db.SaveChangesAsync();

        var resp = new TreinadorResponse(treinador.Id, treinador.Nome, treinador.Email, treinador.CidadeOrigem);
        return CreatedAtAction(nameof(GetById), new { id = treinador.Id }, resp);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TreinadorResponse>> Update(int id, [FromBody] TreinadorRequest req)
    {
        var treinador = await _db.Treinadores.FindAsync(id);
        if (treinador is null) return NotFound();

        if (await _db.Treinadores.AnyAsync(t => t.Email == req.Email && t.Id != id))
            return Conflict(new { erro = "Já existe um Treinador cadastrado com este e-mail." });

        treinador.Nome = req.Nome;
        treinador.Email = req.Email;
        treinador.CidadeOrigem = req.CidadeOrigem;
        await _db.SaveChangesAsync();

        return new TreinadorResponse(treinador.Id, treinador.Nome, treinador.Email, treinador.CidadeOrigem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var treinador = await _db.Treinadores.FindAsync(id);
        if (treinador is null) return NotFound();

        _db.Treinadores.Remove(treinador);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
