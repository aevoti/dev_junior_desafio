using System.ComponentModel.DataAnnotations;

namespace PokemonTraining.Api.DTOs;

public class CriarPokemonRequest
{
    [Required, MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Tipo { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Nivel { get; set; }

    [Range(1, int.MaxValue)]
    public int TreinadorId { get; set; }
}

public class TransferirPokemonRequest
{
    [Range(1, int.MaxValue)]
    public int NovoTreinadorId { get; set; }
}

public record PokemonResponse(
    int Id,
    string Nome,
    string Tipo,
    int Nivel,
    int TreinadorId,
    string TreinadorNome);
