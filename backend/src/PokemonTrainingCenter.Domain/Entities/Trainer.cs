namespace PokemonTrainingCenter.Domain.Entities;

/// <summary>A trainer who owns Pokémon and enrolls them in training plans (FR-001).</summary>
public class Trainer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public ICollection<Pokemon> Pokemons { get; set; } = new List<Pokemon>();
}
