using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Application.DTOs;

public record PokemonDto(int Id, string Nome, TipoPokemon Tipo, int Nivel, int TreinadorId, string TreinadorNome);

public record CriarPokemonRequest(string Nome, TipoPokemon Tipo, int Nivel, int TreinadorId);

public record TransferirPokemonRequest(int NovoTreinadorId);
