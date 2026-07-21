using Microsoft.EntityFrameworkCore;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Infrastructure.Data;

namespace PokemonCenter.Infrastructure.Repositories;

public class PlanoRepository : IPlanoRepository
{
    private readonly AppDbContext _context;

    public PlanoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Plano?> GetByIdAsync(int id)
    {
        return await _context.Planos.FindAsync(id);
    }

    public async Task<List<Plano>> GetAllAsync()
    {
        return await _context.Planos.ToListAsync();
    }
}