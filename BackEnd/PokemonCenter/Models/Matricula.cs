namespace PokemonCenter.Models;

public partial class Matricula
{
    public int Id { get; set; }

    public int PokemonId { get; set; }

    public int PlanoId { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    public string Status { get; set; } = null!;

    public decimal ValorPrimeiraCobranca { get; set; }

    public virtual Plano Plano { get; set; } = null!;

    public virtual Pokemon Pokemon { get; set; } = null!;
}
