using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Enums;

namespace PokemonTraining.Api.Services;

public interface IMatriculaService
{
    Task<IReadOnlyList<MatriculaResponse>> ListarAsync(
        string? busca,
        StatusMatricula? status,
        CancellationToken cancellationToken = default);

    Task<MatriculaResponse> CriarAsync(
        CriarMatriculaRequest request,
        CancellationToken cancellationToken = default);

    Task<MatriculaResponse> CancelarAsync(
        int id,
        CancelarMatriculaRequest request,
        CancellationToken cancellationToken = default);
}
