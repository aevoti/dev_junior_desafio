using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Data;
using PokemonTrainingCenter.DTOs;
using PokemonTrainingCenter.Models;

namespace PokemonTrainingCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PokemonsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PokemonsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<PokemonResponse>> GetAll([FromQuery] int? treinadorId)
    {
        var query = _db.Pokemons.Include(p => p.Treinador).AsQueryable();
        if (treinadorId.HasValue)
            query = query.Where(p => p.TreinadorId == treinadorId.Value);

        return await query
            .Select(p => new PokemonResponse(p.Id, p.Nome, p.Tipo, p.Nivel, p.TreinadorId, p.Treinador.Nome))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PokemonResponse>> GetById(int id)
    {
        var p = await _db.Pokemons.Include(p => p.Treinador).FirstOrDefaultAsync(p => p.Id == id);
        if (p is null) return NotFound();
        return new PokemonResponse(p.Id, p.Nome, p.Tipo, p.Nivel, p.TreinadorId, p.Treinador.Nome);
    }

    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> Create([FromBody] PokemonRequest req)
    {
        if (!await _db.Treinadores.AnyAsync(t => t.Id == req.TreinadorId))
            return BadRequest(new { erro = "Treinador não encontrado." });

        var pokemon = new Pokemon
        {
            Nome = req.Nome,
            Tipo = req.Tipo,
            Nivel = req.Nivel,
            TreinadorId = req.TreinadorId
        };
        _db.Pokemons.Add(pokemon);
        await _db.SaveChangesAsync();

        await _db.Entry(pokemon).Reference(p => p.Treinador).LoadAsync();
        var resp = new PokemonResponse(pokemon.Id, pokemon.Nome, pokemon.Tipo, pokemon.Nivel, pokemon.TreinadorId, pokemon.Treinador.Nome);
        return CreatedAtAction(nameof(GetById), new { id = pokemon.Id }, resp);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PokemonResponse>> Update(int id, [FromBody] PokemonRequest req)
    {
        var pokemon = await _db.Pokemons.Include(p => p.Treinador).FirstOrDefaultAsync(p => p.Id == id);
        if (pokemon is null) return NotFound();

        if (!await _db.Treinadores.AnyAsync(t => t.Id == req.TreinadorId))
            return BadRequest(new { erro = "Treinador não encontrado." });

        pokemon.Nome = req.Nome;
        pokemon.Tipo = req.Tipo;
        pokemon.Nivel = req.Nivel;
        pokemon.TreinadorId = req.TreinadorId;
        await _db.SaveChangesAsync();

        await _db.Entry(pokemon).Reference(p => p.Treinador).LoadAsync();
        return new PokemonResponse(pokemon.Id, pokemon.Nome, pokemon.Tipo, pokemon.Nivel, pokemon.TreinadorId, pokemon.Treinador.Nome);
    }

    // R5: transferir Pokémon para outro Treinador — matrículas ativas permanecem vinculadas ao Pokémon
    [HttpPatch("{id}/transferir")]
    public async Task<ActionResult<PokemonResponse>> Transferir(int id, [FromBody] TransferirPokemonRequest req)
    {
        var pokemon = await _db.Pokemons.Include(p => p.Treinador).FirstOrDefaultAsync(p => p.Id == id);
        if (pokemon is null) return NotFound();

        if (!await _db.Treinadores.AnyAsync(t => t.Id == req.NovoTreinadorId))
            return BadRequest(new { erro = "Novo Treinador não encontrado." });

        pokemon.TreinadorId = req.NovoTreinadorId;
        await _db.SaveChangesAsync();
        await _db.Entry(pokemon).Reference(p => p.Treinador).LoadAsync();

        return new PokemonResponse(pokemon.Id, pokemon.Nome, pokemon.Tipo, pokemon.Nivel, pokemon.TreinadorId, pokemon.Treinador.Nome);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pokemon = await _db.Pokemons.FindAsync(id);
        if (pokemon is null) return NotFound();

        _db.Pokemons.Remove(pokemon);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
