using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Infrastructure.Data;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class PlanoTreinamentoRepository : IPlanoTreinamentoRepository
{
    private readonly AppDbContext _context;

    public PlanoTreinamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<PlanoTreinamento>> ListarAsync() =>
        _context.PlanosTreinamento.AsNoTracking().OrderBy(p => p.Nivel).ToListAsync();

    public Task<PlanoTreinamento?> ObterPorIdAsync(int id) =>
        _context.PlanosTreinamento.FirstOrDefaultAsync(p => p.Id == id);
}
