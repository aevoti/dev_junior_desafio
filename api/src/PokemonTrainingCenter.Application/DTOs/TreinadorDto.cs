namespace PokemonTrainingCenter.Application.DTOs;

public record TreinadorDto(int Id, string Nome, string Email, string CidadeOrigem);

public record CriarTreinadorRequest(string Nome, string Email, string CidadeOrigem);
