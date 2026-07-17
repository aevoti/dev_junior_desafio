using CentroTreinamentoPokemon.DataTransfer.Requests.Pokemon;
using CentroTreinamentoPokemon.DataTransfer.Responses.Pokemon;
using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Exceptions;
using CentroTreinamentoPokemon.Domain.Repositories;

namespace CentroTreinamentoPokemon.Application.Services.Pokemons;

public class PokemonService : IPokemonService
{
    private readonly IPokemonRepository pokemonRepository;
    private readonly ITreinadorRepository treinadorRepository;
    private readonly IUnitOfWork unitOfWork;

    public PokemonService(
        IPokemonRepository pokemonRepository,
        ITreinadorRepository treinadorRepository,
        IUnitOfWork unitOfWork)
    {
        this.pokemonRepository = pokemonRepository;
        this.treinadorRepository = treinadorRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<PokemonResponse> CriarAsync(
        PokemonRequest request)
    {
        Treinador? treinador =
            await treinadorRepository.RecuperarPorIdAsync(
                request.TreinadorId);

        if (treinador is null)
        {
            throw new RegraNegocioException(
                "Treinador não encontrado.");
        }

        Pokemon pokemon = new Pokemon(
            request.Nome,
            request.Tipo,
            request.Nivel,
            treinador);

        await pokemonRepository.InserirAsync(pokemon);
        await unitOfWork.CommitAsync();

        return MapearResponse(pokemon);
    }

    public async Task<PokemonResponse?> ObterPorIdAsync(int id)
    {
        Pokemon? pokemon =
            await pokemonRepository.RecuperarPorIdAsync(id);

        if (pokemon is null)
            return null;

        return MapearResponse(pokemon);
    }

    public async Task<IEnumerable<PokemonResponse>> ListarAsync()
    {
        IEnumerable<Pokemon> pokemons =
            await pokemonRepository.ListarAsync();

        return pokemons.Select(MapearResponse);
    }

    public async Task<IEnumerable<PokemonResponse>>
        ListarPorTreinadorAsync(int treinadorId)
    {
        Treinador? treinador =
            await treinadorRepository.RecuperarPorIdAsync(
                treinadorId);

        if (treinador is null)
        {
            throw new RegraNegocioException(
                "Treinador não encontrado.");
        }

        IEnumerable<Pokemon> pokemons =
            await pokemonRepository.ListarPorTreinadorAsync(
                treinadorId);

        return pokemons.Select(MapearResponse);
    }

    public async Task<PokemonResponse?> TransferirAsync(
        int id,
        TransferirPokemonRequest request)
    {
        Pokemon? pokemon =
            await pokemonRepository.RecuperarPorIdAsync(id);

        if (pokemon is null)
            return null;

        Treinador? novoTreinador =
            await treinadorRepository.RecuperarPorIdAsync(
                request.TreinadorId);

        if (novoTreinador is null)
        {
            throw new RegraNegocioException(
                "Novo treinador não encontrado.");
        }

        pokemon.TransferirTreinador(novoTreinador);

        await unitOfWork.CommitAsync();

        return MapearResponse(pokemon);
    }

    private static PokemonResponse MapearResponse(
        Pokemon pokemon)
    {
        return new PokemonResponse
        {
            Id = pokemon.Id,
            Nome = pokemon.Nome,
            Tipo = pokemon.Tipo,
            Nivel = pokemon.Nivel,
            TreinadorId = pokemon.TreinadorId,
            TreinadorNome = pokemon.Treinador.Nome
        };
    }
}