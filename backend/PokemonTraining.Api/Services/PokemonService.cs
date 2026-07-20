using Microsoft.EntityFrameworkCore;
using PokemonTraining.Api.Data;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Entities;
using PokemonTraining.Api.Exceptions;

namespace PokemonTraining.Api.Services;

public class PokemonService(PokemonTrainingDbContext context) : IPokemonService
{
    public async Task<IReadOnlyList<PokemonResponse>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Pokemons
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new PokemonResponse(
                x.Id,
                x.Nome,
                x.Tipo,
                x.Nivel,
                x.TreinadorId,
                x.Treinador.Nome))
            .ToListAsync(cancellationToken);

    public async Task<PokemonResponse> CriarAsync(
        CriarPokemonRequest request,
        CancellationToken cancellationToken = default)
    {
        var treinador = await context.Treinadores.FindAsync([request.TreinadorId], cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Treinador não encontrado.");

        if (request.Nivel is < 1 or > 100)
        {
            throw new RegraNegocioException("O nível do Pokémon deve estar entre 1 e 100.");
        }

        var pokemon = new Pokemon
        {
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo.Trim(),
            Nivel = request.Nivel,
            TreinadorId = treinador.Id,
            Treinador = treinador
        };

        context.Pokemons.Add(pokemon);
        await context.SaveChangesAsync(cancellationToken);

        return Mapear(pokemon);
    }

    public async Task<PokemonResponse> TransferirAsync(
        int pokemonId,
        TransferirPokemonRequest request,
        CancellationToken cancellationToken = default)
    {
        var pokemon = await context.Pokemons
            .Include(x => x.Treinador)
            .SingleOrDefaultAsync(x => x.Id == pokemonId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Pokémon não encontrado.");

        var novoTreinador = await context.Treinadores.FindAsync([request.NovoTreinadorId], cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Treinador não encontrado.");

        pokemon.TreinadorId = novoTreinador.Id;
        pokemon.Treinador = novoTreinador;
        await context.SaveChangesAsync(cancellationToken);

        return Mapear(pokemon);
    }

    private static PokemonResponse Mapear(Pokemon pokemon) => new(
        pokemon.Id,
        pokemon.Nome,
        pokemon.Tipo,
        pokemon.Nivel,
        pokemon.TreinadorId,
        pokemon.Treinador.Nome);
}
