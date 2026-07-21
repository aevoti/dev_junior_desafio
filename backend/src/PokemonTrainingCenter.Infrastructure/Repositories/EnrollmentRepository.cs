using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Repositories;
using PokemonTrainingCenter.Infrastructure.Persistence;

namespace PokemonTrainingCenter.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _db;

    public EnrollmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> HasOpenOrActiveAsync(int pokemonId, DateTime now, CancellationToken ct = default) =>
        _db.Enrollments.AnyAsync(
            e => e.PokemonId == pokemonId && (e.EndDate == null || e.EndDate.Value >= now), ct);

    public Task<Enrollment?> GetActiveByPokemonIdAsync(int pokemonId, DateTime now, CancellationToken ct = default) =>
        _db.Enrollments
            .Where(e => e.PokemonId == pokemonId && (e.EndDate == null || e.EndDate.Value >= now))
            .FirstOrDefaultAsync(ct);

    public Task<List<Enrollment>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        _db.Enrollments
            .Include(e => e.Pokemon!)
            .Include(e => e.Trainer)
            .Include(e => e.TrainingPlan)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(ct);

    public void Add(Enrollment enrollment) => _db.Enrollments.Add(enrollment);
}
