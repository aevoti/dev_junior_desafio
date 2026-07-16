using PokemonTraining.Api.DTOs;

namespace PokemonTraining.Api.Services;

public interface ITreinadorService
{
    Task<IReadOnlyList<TreinadorResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<TreinadorResponse> CriarAsync(CriarTreinadorRequest request, CancellationToken cancellationToken = default);
}
