namespace PokemonCenter.Domain.Entities;

public class Plano
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal ValorMensal { get; set; }
    public int NivelMinimoRequerido { get; set; }
}