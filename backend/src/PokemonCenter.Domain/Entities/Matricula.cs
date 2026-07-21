using PokemonCenter.Domain.Enums;

namespace PokemonCenter.Domain.Entities;

public class Matricula
{
    public int Id { get; set; }
    public int PokemonId { get; set; }
    public int PlanoId { get; set; }
    public DateTime DataInicio { get; set; }
    public StatusMatricula Status { get; set; }
    public decimal ValorMensal { get; set; }

    public Pokemon? Pokemon { get; set; }
    public Plano? Plano { get; set; }
}