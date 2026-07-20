using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Domain.Entities;

/// <summary>A Pokémon owned by a Trainer (FR-003). Ownership can change via transfer (R5).</summary>
public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PokemonType Type { get; set; }
    public int Level { get; set; }

    public int TrainerId { get; set; }
    public Trainer? Trainer { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
