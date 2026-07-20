using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Domain.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>True when the Pokémon has an open or still-active enrollment (R1).</summary>
    Task<bool> HasOpenOrActiveAsync(int pokemonId, DateTime now, CancellationToken ct = default);

    /// <summary>The Pokémon's open or still-active enrollment, if any (R5).</summary>
    Task<Enrollment?> GetActiveByPokemonIdAsync(int pokemonId, DateTime now, CancellationToken ct = default);

    /// <summary>All enrollments with Pokemon, Trainer and TrainingPlan loaded, newest first (US3).</summary>
    Task<List<Enrollment>> GetAllWithDetailsAsync(CancellationToken ct = default);

    void Add(Enrollment enrollment);
}
