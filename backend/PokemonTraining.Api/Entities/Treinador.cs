namespace PokemonTraining.Api.Entities;

public class Treinador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CidadeOrigem { get; set; } = string.Empty;
    public ICollection<Pokemon> Pokemons { get; set; } = new List<Pokemon>();
}
