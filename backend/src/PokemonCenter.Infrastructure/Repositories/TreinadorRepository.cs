using Microsoft.EntityFrameworkCore;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Infrastructure.Data;

namespace PokemonCenter.Infrastructure.Repositories;

public class TreinadorRepository : ITreinadorRepository
{
    private readonly AppDbContext _context;

    public TreinadorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Treinador?> GetByIdAsync(int id)
    {
        return await _context.Treinadores.FindAsync(id);
    }

    public async Task<Treinador?> GetByEmailAsync(string email)
    {
        return await _context.Treinadores.FirstOrDefaultAsync(t => t.Email == email);
    }

    public async Task<List<Treinador>> GetAllAsync()
    {
        return await _context.Treinadores.ToListAsync();
    }

    public async Task<Treinador> AddAsync(Treinador treinador)
    {
        _context.Treinadores.Add(treinador);
        await _context.SaveChangesAsync();
        return treinador;
    }
}