using PokemonTraining.Api.Enums;

namespace PokemonTraining.Api.Entities;

public class Matricula
{
    public int Id { get; set; }
    public int PokemonId { get; set; }
    public Pokemon Pokemon { get; set; } = null!;
    public int PlanoTreinamentoId { get; set; }
    public PlanoTreinamento PlanoTreinamento { get; set; } = null!;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public StatusMatricula Status { get; set; }
    public decimal ValorMensal { get; set; }
    public string? MotivoEncerramento { get; set; }
}
