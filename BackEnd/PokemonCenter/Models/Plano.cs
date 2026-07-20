namespace PokemonCenter.Models;

public partial class Plano
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Descricao { get; set; } = null!;

    public decimal ValorMensal { get; set; }

    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
