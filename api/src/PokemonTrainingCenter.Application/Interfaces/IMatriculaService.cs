using PokemonTrainingCenter.Application.DTOs;
using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Application.Interfaces;

/// <summary>Orquestra as regras de negócio de matrícula (R1 a R4; R5 fica em IPokemonService).</summary>
public interface IMatriculaService
{
    Task<List<MatriculaDto>> ListarAsync(string? nomeBusca, StatusMatricula? status);

    /// <summary>Cria uma matrícula. Deve rejeitar se o Pokémon já tiver matrícula ativa (R1)
    /// ou se o nível mínimo do plano não for atendido (R3).</summary>
    Task<MatriculaDto> CriarAsync(CriarMatriculaRequest request);

    /// <summary>Calcula o pro-rata do upgrade (R2) sem persistir — para a UI exibir o valor
    /// da primeira cobrança antes do Treinador confirmar.</summary>
    Task<UpgradeMatriculaResponse> SimularUpgradeAsync(int matriculaId, UpgradeMatriculaRequest request);

    /// <summary>Encerra a matrícula atual e cria uma nova no plano superior, efetivando
    /// o upgrade calculado por <see cref="SimularUpgradeAsync"/>. Deve rejeitar downgrade.</summary>
    Task<UpgradeMatriculaResponse> UpgradeAsync(int matriculaId, UpgradeMatriculaRequest request);

    Task CancelarAsync(int matriculaId);
}
