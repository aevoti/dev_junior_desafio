using PokemonTraining.Api.DTOs;

namespace PokemonTraining.Api.Services;

public interface IPlanoTreinamentoService
{
    Task<IReadOnlyList<PlanoTreinamentoResponse>> ListarAsync(CancellationToken cancellationToken = default);
}
