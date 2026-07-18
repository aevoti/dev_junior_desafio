using System.ComponentModel.DataAnnotations;

namespace PokemonTrainingCenter.DTOs;

public record PokemonRequest(
    [Required, MaxLength(100)] string Nome,
    [Required, MaxLength(50)] string Tipo,
    [Range(1, 100)] int Nivel,
    int TreinadorId
);

public record PokemonResponse(int Id, string Nome, string Tipo, int Nivel, int TreinadorId, string NomeTreinador);

public record TransferirPokemonRequest(int NovoTreinadorId);
