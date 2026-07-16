using System.ComponentModel.DataAnnotations;

namespace PokemonTraining.Api.DTOs;

public class UpgradeMatriculaRequest
{
    [Range(1, int.MaxValue)]
    public int NovoPlanoTreinamentoId { get; set; }
}

public record SimulacaoUpgradeResponse(
    int MatriculaAtualId,
    int PlanoAtualId,
    string PlanoAtualNome,
    int NovoPlanoId,
    string NovoPlanoNome,
    int DiasRestantes,
    decimal CreditoPlanoAnterior,
    decimal CustoProporcionalNovoPlano,
    decimal PrimeiraCobranca,
    DateTime DataUpgrade);

public record UpgradeMatriculaResponse(
    int MatriculaAnteriorId,
    int NovaMatriculaId,
    int PokemonId,
    string PokemonNome,
    int PlanoAnteriorId,
    string PlanoAnteriorNome,
    int NovoPlanoId,
    string NovoPlanoNome,
    int DiasRestantes,
    decimal CreditoPlanoAnterior,
    decimal CustoProporcionalNovoPlano,
    decimal PrimeiraCobranca,
    DateTime DataUpgrade);
