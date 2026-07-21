using Microsoft.EntityFrameworkCore;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Domain.Enums;
using PokemonCenter.Infrastructure.Data;

namespace PokemonCenter.Infrastructure.Repositories;

public class MatriculaRepository : IMatriculaRepository
{
    private readonly AppDbContext _context;

    public MatriculaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Matricula?> GetByIdAsync(int id)
    {
        return await _context.Matriculas
            .Include(m => m.Pokemon)
                .ThenInclude(p => p!.Treinador)
            .Include(m => m.Plano)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Matricula>> GetAllAsync(string? busca, StatusMatricula? status)
    {
        var query = _context.Matriculas
            .Include(m => m.Pokemon)
                .ThenInclude(p => p!.Treinador)
            .Include(m => m.Plano)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(m =>
                m.Pokemon!.Nome.Contains(busca) ||
                m.Pokemon.Treinador!.Nome.Contains(busca));
        }

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        return await query.ToListAsync();
    }

    public async Task<Matricula?> GetMatriculaAtivaByPokemonIdAsync(int pokemonId)
    {
        return await _context.Matriculas
            .Include(m => m.Plano)
            .FirstOrDefaultAsync(m => m.PokemonId == pokemonId && m.Status == StatusMatricula.Ativa);
    }

    public async Task<Matricula> AddAsync(Matricula matricula)
    {
        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();
        return matricula;
    }

    public async Task UpdateAsync(Matricula matricula)
    {
        _context.Matriculas.Update(matricula);
        await _context.SaveChangesAsync();
    }
}