using System.ComponentModel.DataAnnotations;
using PokemonTrainingCenter.Models;

namespace PokemonTrainingCenter.DTOs;

public record MatriculaRequest(
    int PokemonId,
    [Required] PlanoTreinamento Plano
);

public record MatriculaResponse(
    int Id,
    int PokemonId,
    string NomePokemon,
    string NomeTreinador,
    string Plano,
    DateTime DataInicio,
    string Status,
    decimal ValorMensal
);

public record UpgradeRequest(PlanoTreinamento NovoPlano);

public record UpgradePreviewResponse(
    decimal PrimeiraCobranca,
    decimal CreditoPlanoAntigo,
    decimal CustoNovoPlanoDiasRestantes,
    int DiasRestantes,
    string Descricao
);
