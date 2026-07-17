using PokemonTraining.Api.DTOs;

namespace PokemonTraining.Api.Services;

public interface IPokemonService
{
    Task<IReadOnlyList<PokemonResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<PokemonResponse> CriarAsync(CriarPokemonRequest request, CancellationToken cancellationToken = default);
    Task<PokemonResponse> TransferirAsync(int pokemonId, TransferirPokemonRequest request, CancellationToken cancellationToken = default);
}
