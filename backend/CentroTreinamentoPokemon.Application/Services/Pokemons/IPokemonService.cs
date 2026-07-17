using CentroTreinamentoPokemon.DataTransfer.Requests.Pokemon;
using CentroTreinamentoPokemon.DataTransfer.Responses.Pokemon;

namespace CentroTreinamentoPokemon.Application.Services.Pokemons;

public interface IPokemonService
{
    Task<PokemonResponse> CriarAsync(PokemonRequest request);

    Task<PokemonResponse?> ObterPorIdAsync(int id);

    Task<IEnumerable<PokemonResponse>> ListarAsync();

    Task<IEnumerable<PokemonResponse>> ListarPorTreinadorAsync(
        int treinadorId);

    Task<PokemonResponse?> TransferirAsync(
        int id,
        TransferirPokemonRequest request);
}