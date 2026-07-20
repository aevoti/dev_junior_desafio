using System.ComponentModel.DataAnnotations;

namespace PokemonCenter.DTOs;

public record CriarMatriculaRequest(
        [property: Range(1, int.MaxValue)] int PokemonId,
        [property: Range(1, int.MaxValue)] int PlanoId
        );

public record UpgradeMatriculaRequest(
    [property: Range(1, int.MaxValue)] int NovoPlanoId
);

public record SimulacaoUpgradeResponse(decimal PrimeiraCobranca);

public record MatriculaResponse(
        int Id,
        string Status,
        DateOnly DataInicio,
        DateOnly? DataFim,
        decimal ValorPrimeiraCobranca,
        int PokemonId,
        string PokemonNome,
        int Nivel,
        int TreinadorId,
        string TreinadorNome,
        int PlanoId,
        string PlanoNome,
        decimal PlanoValorMensal
        );
