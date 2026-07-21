using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Repositories;
using PokemonTrainingCenter.Infrastructure.Persistence;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class PokemonRepository : IPokemonRepository
{
    private readonly AppDbContext _db;

    public PokemonRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Pokemon?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Pokemons.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Pokemon>> GetAllWithTrainerAsync(CancellationToken ct = default) =>
        _db.Pokemons.Include(p => p.Trainer).OrderBy(p => p.Name).ToListAsync(ct);

    public void Add(Pokemon pokemon) => _db.Pokemons.Add(pokemon);
}
