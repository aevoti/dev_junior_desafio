using PokemonTrainingCenter.Application.DTOs;
using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Application.Interfaces;

/// <summary>
/// Orquestra as regras de negócio de matrícula (R1 a R5).
/// A implementação (MatriculaService) ainda é um stub — ver TODOs na classe.
/// </summary>
public interface IMatriculaService
{
    Task<List<MatriculaDto>> ListarAsync(string? nomeBusca, StatusMatricula? status);

    /// <summary>Cria uma matrícula. Deve rejeitar se o Pokémon já tiver matrícula ativa (R1)
    /// ou se o nível mínimo do plano não for atendido (R3).</summary>
    Task<MatriculaDto> CriarAsync(CriarMatriculaRequest request);

    /// <summary>Encerra a matrícula atual e cria uma nova no plano superior, calculando
    /// o valor pro-rata da primeira cobrança (R2). Deve rejeitar downgrade.</summary>
    Task<UpgradeMatriculaResponse> UpgradeAsync(int matriculaId, UpgradeMatriculaRequest request);

    Task CancelarAsync(int matriculaId);
}
