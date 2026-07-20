using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Repositories;
using CentroTreinamentoPokemon.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroTreinamentoPokemon.Infrastructure.Repositories;

public class PokemonRepository : IPokemonRepository
{
    private readonly CentroTreinamentoPokemonContext _context;

    public PokemonRepository(
        CentroTreinamentoPokemonContext context)
    {
        _context = context;
    }

    public async Task<Pokemon?> RecuperarPorIdAsync(int id)
    {
        Pokemon? pokemon = await _context.Pokemons
            .Include(pokemon => pokemon.Treinador)
            .Include(pokemon => pokemon.Matriculas)
                .ThenInclude(matricula => matricula.PlanoTreinamento)
            .FirstOrDefaultAsync(pokemon => pokemon.Id == id);

        return pokemon;
    }

    public async Task<IList<Pokemon>> ListarAsync()
    {
        IList<Pokemon> pokemons = await _context.Pokemons
            .AsNoTracking()
            .Include(pokemon => pokemon.Treinador)
            .OrderBy(pokemon => pokemon.Nome)
            .ToListAsync();

        return pokemons;
    }

    public async Task<IList<Pokemon>> ListarPorTreinadorAsync(
    int treinadorId)
    {
        IList<Pokemon> pokemons = await _context.Pokemons
            .AsNoTracking()
            .Include(pokemon => pokemon.Treinador)
            .Where(pokemon =>
                pokemon.TreinadorId == treinadorId)
            .OrderBy(pokemon => pokemon.Nome)
            .ToListAsync();

        return pokemons;
    }

    public async Task InserirAsync(Pokemon pokemon)
    {
        await _context.Pokemons.AddAsync(pokemon);
    }
}