namespace PokemonCenter.Models;

public partial class Treinador
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string CidadeDeOrigem { get; set; } = null!;

    public virtual ICollection<Pokemon> Pokemons { get; set; } = new List<Pokemon>();
}
