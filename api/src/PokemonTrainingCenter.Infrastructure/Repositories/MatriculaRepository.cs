using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;
using PokemonTrainingCenter.Infrastructure.Data;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class MatriculaRepository : IMatriculaRepository
{
    private readonly AppDbContext _context;

    public MatriculaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Matricula>> ListarAsync(string? nomeBusca, StatusMatricula? status)
    {
        var query = _context.Matriculas
            .Include(m => m.Pokemon).ThenInclude(p => p!.Treinador)
            .Include(m => m.PlanoTreinamento)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nomeBusca))
        {
            query = query.Where(m =>
                m.Pokemon!.Nome.Contains(nomeBusca) ||
                m.Pokemon!.Treinador!.Nome.Contains(nomeBusca));
        }

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        return await query.ToListAsync();
    }

    public Task<Matricula?> ObterPorIdAsync(int id) =>
        _context.Matriculas
            .Include(m => m.Pokemon)
            .Include(m => m.PlanoTreinamento)
            .FirstOrDefaultAsync(m => m.Id == id);

    public Task<Matricula?> ObterAtivaPorPokemonAsync(int pokemonId) =>
        _context.Matriculas
            .Include(m => m.PlanoTreinamento)
            .FirstOrDefaultAsync(m => m.PokemonId == pokemonId && m.Status == StatusMatricula.Ativa);

    public async Task AdicionarAsync(Matricula matricula)
    {
        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Matricula matricula)
    {
        _context.Matriculas.Update(matricula);
        await _context.SaveChangesAsync();
    }
}
