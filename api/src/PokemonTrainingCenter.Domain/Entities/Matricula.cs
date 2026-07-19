using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Domain.Entities;

public class Matricula
{
    public int Id { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public StatusMatricula Status { get; set; }
    public decimal ValorMensal { get; set; }

    public int PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }

    public int PlanoTreinamentoId { get; set; }
    public PlanoTreinamento? PlanoTreinamento { get; set; }

    /// <summary>
    /// Preenchido quando esta matrícula é resultado de um upgrade (R2), apontando para a matrícula encerrada.
    /// </summary>
    public int? MatriculaOrigemId { get; set; }
}
