using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Repositories;
using CentroTreinamentoPokemon.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroTreinamentoPokemon.Infrastructure.Repositories;

public class TreinadorRepository : ITreinadorRepository
{
    private readonly CentroTreinamentoPokemonContext _context;

    public TreinadorRepository(
        CentroTreinamentoPokemonContext context)
    {
        _context = context;
    }

    public async Task<Treinador?> RecuperarPorIdAsync(int id)
    {
        Treinador? treinador = await _context.Treinadores
            .Include(treinador => treinador.Pokemons)
            .FirstOrDefaultAsync(treinador => treinador.Id == id);

        return treinador;
    }

    public async Task<Treinador?> RecuperarPorEmailAsync(
        string email)
    {
        Treinador? treinador = await _context.Treinadores
            .FirstOrDefaultAsync(
                treinador => treinador.Email == email);

        return treinador;
    }

    public async Task<IList<Treinador>> ListarAsync()
    {
        IList<Treinador> treinadores =
            await _context.Treinadores
                .AsNoTracking()
                .OrderBy(treinador => treinador.Nome)
                .ToListAsync();

        return treinadores;
    }

    public async Task InserirAsync(Treinador treinador)
    {
        await _context.Treinadores.AddAsync(treinador);
    }

    public async Task<bool> ExistePorEmailAsync(string email)
    {
        bool existe = await _context.Treinadores
            .AnyAsync(treinador => treinador.Email == email);

        return existe;
    }
}