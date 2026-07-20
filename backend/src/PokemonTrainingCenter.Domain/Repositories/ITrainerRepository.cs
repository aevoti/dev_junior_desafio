using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Domain.Repositories;

public interface ITrainerRepository
{
    Task<Trainer?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<List<Trainer>> GetAllOrderedByNameAsync(CancellationToken ct = default);
    void Add(Trainer trainer);
}
