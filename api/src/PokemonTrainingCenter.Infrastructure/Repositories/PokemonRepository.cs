using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Infrastructure.Data;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class PokemonRepository : IPokemonRepository
{
    private readonly AppDbContext _context;

    public PokemonRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Pokemon>> ListarAsync() =>
        _context.Pokemons.Include(p => p.Treinador).AsNoTracking().ToListAsync();

    public Task<Pokemon?> ObterPorIdAsync(int id) =>
        _context.Pokemons.Include(p => p.Treinador).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AdicionarAsync(Pokemon pokemon)
    {
        _context.Pokemons.Add(pokemon);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Pokemon pokemon)
    {
        _context.Pokemons.Update(pokemon);
        await _context.SaveChangesAsync();
    }
}
