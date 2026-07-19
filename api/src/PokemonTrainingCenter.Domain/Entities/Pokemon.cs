using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Domain.Entities;

public class Pokemon
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoPokemon Tipo { get; set; }
    public int Nivel { get; set; }

    public int TreinadorId { get; set; }
    public Treinador? Treinador { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
