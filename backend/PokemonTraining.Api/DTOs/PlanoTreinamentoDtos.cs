namespace PokemonTraining.Api.DTOs;

public record PlanoTreinamentoResponse(
    int Id,
    string Nome,
    decimal ValorMensal,
    string Descricao,
    int Ordem,
    int NivelMinimo);
