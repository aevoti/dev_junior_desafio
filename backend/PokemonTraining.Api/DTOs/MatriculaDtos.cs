using System.ComponentModel.DataAnnotations;
using PokemonTraining.Api.Enums;

namespace PokemonTraining.Api.DTOs;

public class CriarMatriculaRequest
{
    [Range(1, int.MaxValue)]
    public int PokemonId { get; set; }

    [Range(1, int.MaxValue)]
    public int PlanoTreinamentoId { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }
}

public class CancelarMatriculaRequest
{
    [MaxLength(500)]
    public string? Motivo { get; set; }
}

public record MatriculaResponse(
    int Id,
    int PokemonId,
    string PokemonNome,
    int TreinadorId,
    string TreinadorNome,
    int PlanoTreinamentoId,
    string PlanoNome,
    DateTime DataInicio,
    DateTime? DataFim,
    StatusMatricula Status,
    decimal ValorMensal,
    string? MotivoEncerramento);
