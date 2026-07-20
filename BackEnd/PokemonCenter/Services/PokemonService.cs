using Microsoft.EntityFrameworkCore;
using PokemonCenter.DTOs;
using PokemonCenter.Exceptions;
using PokemonCenter.Models;

namespace PokemonCenter.Services;

public class PokemonService
{
    private readonly PokemonCenterContext _context;

    public PokemonService(PokemonCenterContext context)
    {
        _context = context;
    }

    public async Task<PokemonResponse> Criar(CriarPokemonRequest novoPokemon)
    {
        var treinador = await _context.Treinadors.FindAsync(novoPokemon.TreinadorId);

        if (treinador is null) throw new RecursoNaoEncontradoException("Treinador não existente.");

        var pokemon = new Pokemon
        {
            Nome = novoPokemon.Nome,
            Tipo = novoPokemon.Tipo,
            Nivel = novoPokemon.Nivel,
            TreinadorId = novoPokemon.TreinadorId
        };

        _context.Pokemons.Add(pokemon);
        await _context.SaveChangesAsync();

        return new PokemonResponse(pokemon.Id, pokemon.Nome, pokemon.Tipo, pokemon.Nivel, pokemon.TreinadorId);
    }

    public async Task<List<PokemonResponse>> ListarTodos()
    {
        return await _context.Pokemons.Select(t => new PokemonResponse(t.Id, t.Nome, t.Tipo, t.Nivel, t.TreinadorId)).ToListAsync();
    }

    public async Task<PokemonResponse> ObterPorId(int id)
    {
        var pokemon = await _context.Pokemons.FindAsync(id);

        if (pokemon is null)
        {
            throw new RecursoNaoEncontradoException("Pokémon não existe");
        }

        return new PokemonResponse(pokemon.Id, pokemon.Nome, pokemon.Tipo, pokemon.Nivel, pokemon.TreinadorId);
    }

    public async Task<PokemonResponse> Transferir(int pokemonId, int novoTreinadorId)
    {
        var pokemon = await _context.Pokemons.FindAsync(pokemonId);

        if (pokemon is null)
        {
            throw new RecursoNaoEncontradoException("Pokémon não existe");
        }

        if (!await _context.Treinadors.AnyAsync(t => t.Id == novoTreinadorId))
        {
            throw new RecursoNaoEncontradoException("Treinador não existe");
        }

        pokemon.TreinadorId = novoTreinadorId;

        await _context.SaveChangesAsync();

        return new PokemonResponse(pokemon.Id, pokemon.Nome, pokemon.Tipo, pokemon.Nivel, pokemon.TreinadorId);
    }
}
