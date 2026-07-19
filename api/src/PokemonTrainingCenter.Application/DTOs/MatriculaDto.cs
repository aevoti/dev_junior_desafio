using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Application.DTOs;

public record MatriculaDto(
    int Id,
    int PokemonId,
    string PokemonNome,
    string TreinadorNome,
    int PlanoTreinamentoId,
    string PlanoTreinamentoNome,
    DateTime DataInicio,
    DateTime? DataFim,
    StatusMatricula Status,
    decimal ValorMensal);

public record CriarMatriculaRequest(int PokemonId, int PlanoTreinamentoId, DateTime DataInicio);

/// <summary>Request do fluxo de upgrade (R2).</summary>
public record UpgradeMatriculaRequest(int NovoPlanoTreinamentoId, DateTime DataUpgrade);

/// <summary>Resposta com o valor calculado pro-rata da primeira cobrança do novo plano (R2).</summary>
public record UpgradeMatriculaResponse(
    int NovaMatriculaId,
    decimal CreditoPlanoAntigo,
    decimal CustoNovoPlanoRestante,
    decimal ValorPrimeiraCobranca);
