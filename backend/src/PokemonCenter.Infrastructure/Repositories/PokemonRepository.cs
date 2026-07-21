using Microsoft.EntityFrameworkCore;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Infrastructure.Data;

namespace PokemonCenter.Infrastructure.Repositories;

public class PokemonRepository : IPokemonRepository
{
    private readonly AppDbContext _context;

    public PokemonRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Pokemon?> GetByIdAsync(int id)
    {
        return await _context.Pokemons
            .Include(p => p.Treinador)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Pokemon>> GetAllAsync()
    {
        return await _context.Pokemons
            .Include(p => p.Treinador)
            .ToListAsync();
    }

    public async Task<Pokemon> AddAsync(Pokemon pokemon)
    {
        _context.Pokemons.Add(pokemon);
        await _context.SaveChangesAsync();
        return pokemon;
    }

    public async Task UpdateAsync(Pokemon pokemon)
    {
        _context.Pokemons.Update(pokemon);
        await _context.SaveChangesAsync();
    }
}