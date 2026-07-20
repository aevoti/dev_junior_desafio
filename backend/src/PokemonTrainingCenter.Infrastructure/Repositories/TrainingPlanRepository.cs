using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Repositories;
using PokemonTrainingCenter.Infrastructure.Persistence;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class TrainingPlanRepository : ITrainingPlanRepository
{
    private readonly AppDbContext _db;

    public TrainingPlanRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<TrainingPlan?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.TrainingPlans.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<TrainingPlan>> GetAllOrderedByPriceAsync(CancellationToken ct = default) =>
        _db.TrainingPlans.OrderBy(p => p.MonthlyPrice).ToListAsync(ct);
}
