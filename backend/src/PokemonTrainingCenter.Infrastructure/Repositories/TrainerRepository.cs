using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Repositories;
using PokemonTrainingCenter.Infrastructure.Persistence;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class TrainerRepository : ITrainerRepository
{
    private readonly AppDbContext _db;

    public TrainerRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Trainer?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Trainers.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Trainers.AnyAsync(t => t.Email == email, ct);

    public Task<List<Trainer>> GetAllOrderedByNameAsync(CancellationToken ct = default) =>
        _db.Trainers.OrderBy(t => t.Name).ToListAsync(ct);

    public void Add(Trainer trainer) => _db.Trainers.Add(trainer);
}
