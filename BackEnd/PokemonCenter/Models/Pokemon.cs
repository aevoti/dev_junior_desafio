namespace PokemonCenter.Models;

public partial class Pokemon
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public int Nivel { get; set; }

    public int TreinadorId { get; set; }

    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    public virtual Treinador Treinador { get; set; } = null!;
}
