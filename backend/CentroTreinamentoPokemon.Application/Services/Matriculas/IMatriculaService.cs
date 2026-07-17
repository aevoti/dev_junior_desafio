using CentroTreinamentoPokemon.DataTransfer.Requests.Matricula;
using CentroTreinamentoPokemon.DataTransfer.Responses.Matricula;
using CentroTreinamentoPokemon.Domain.Enums;

namespace CentroTreinamentoPokemon.Application.Services.Matriculas;

public interface IMatriculaService
{
    Task<MatriculaResponse> CriarAsync(
        MatriculaRequest request);

    Task<MatriculaResponse?> ObterPorIdAsync(int id);

    Task<IEnumerable<MatriculaResponse>> ListarAsync(
        string? busca,
        StatusMatriculaEnum? status);

    Task<MatriculaResponse?> CancelarAsync(
        int id,
        CancelarMatriculaRequest request);

    Task<UpgradeMatriculaResponse?> SimularUpgradeAsync(
        int id,
        UpgradeMatriculaRequest request);

    Task<UpgradeMatriculaResponse?> ConfirmarUpgradeAsync(
        int id,
        UpgradeMatriculaRequest request);
}