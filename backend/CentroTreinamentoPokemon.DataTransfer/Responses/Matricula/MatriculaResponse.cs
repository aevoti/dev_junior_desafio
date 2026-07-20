using CentroTreinamentoPokemon.Domain.Enums;

namespace CentroTreinamentoPokemon.DataTransfer.Responses.Matricula;

public class MatriculaResponse
{
    public int Id { get; set; }

    public int PokemonId { get; set; }

    public string PokemonNome { get; set; } = string.Empty;

    public int TreinadorId { get; set; }

    public string TreinadorNome { get; set; } = string.Empty;

    public int PlanoTreinamentoId { get; set; }

    public string PlanoTreinamentoNome { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }

    public DateTime? DataEncerramento { get; set; }

    public StatusMatriculaEnum Status { get; set; }

    public decimal ValorMensal { get; set; }
}