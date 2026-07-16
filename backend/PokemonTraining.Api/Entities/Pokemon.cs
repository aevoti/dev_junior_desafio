namespace PokemonTraining.Api.Entities;

public class Pokemon
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public int TreinadorId { get; set; }
    public Treinador Treinador { get; set; } = null!;
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
