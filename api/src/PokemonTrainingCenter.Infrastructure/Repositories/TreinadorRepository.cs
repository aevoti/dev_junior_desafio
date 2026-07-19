using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Infrastructure.Data;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class TreinadorRepository : ITreinadorRepository
{
    private readonly AppDbContext _context;

    public TreinadorRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Treinador>> ListarAsync() =>
        _context.Treinadores.AsNoTracking().ToListAsync();

    public Task<Treinador?> ObterPorIdAsync(int id) =>
        _context.Treinadores.FirstOrDefaultAsync(t => t.Id == id);

    public Task<Treinador?> ObterPorEmailAsync(string email) =>
        _context.Treinadores.FirstOrDefaultAsync(t => t.Email == email);

    public async Task AdicionarAsync(Treinador treinador)
    {
        _context.Treinadores.Add(treinador);
        await _context.SaveChangesAsync();
    }
}
