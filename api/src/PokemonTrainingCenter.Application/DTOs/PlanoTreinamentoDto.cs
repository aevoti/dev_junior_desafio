namespace PokemonTrainingCenter.Application.DTOs;

public record PlanoTreinamentoDto(int Id, string Nome, string Descricao, decimal ValorMensal, int Nivel, int NivelMinimoPokemon);
