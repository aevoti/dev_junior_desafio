using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Domain.Repositories;

public interface ITrainingPlanRepository
{
    Task<TrainingPlan?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<TrainingPlan>> GetAllOrderedByPriceAsync(CancellationToken ct = default);
}
