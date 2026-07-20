using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Exceptions;
using PokemonTrainingCenter.Domain.Persistence;

namespace PokemonTrainingCenter.Domain.Services;

/// <summary>
/// Implements the enrollment lifecycle business rules — R1 (single active
/// enrollment), R2 (upgrade pro-rata), R3 (Elite dos 4 minimum level), R4
/// (cancellation) and R5 (transfer). See spec.md FR-005 through FR-015.
/// </summary>
public class EnrollmentService
{
    /// <summary>Plans below this level cannot be used for "Elite dos 4" (R3).</summary>
    public const int EliteDosQuatroMinimumLevel = 50;
    private const string EliteDosQuatroPlanName = "Elite dos 4";

    private readonly AppDbContext _db;

    public EnrollmentService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Creates a new active enrollment for a Pokémon in a plan (FR-005, FR-006, FR-007).</summary>
    public async Task<Enrollment> CreateEnrollmentAsync(int pokemonId, int trainingPlanId, CancellationToken ct = default)
    {
        var pokemon = await _db.Pokemons.FirstOrDefaultAsync(p => p.Id == pokemonId, ct)
            ?? throw new DomainValidationException("Pokémon não encontrado.", 404);

        var plan = await _db.TrainingPlans.FirstOrDefaultAsync(p => p.Id == trainingPlanId, ct)
            ?? throw new DomainValidationException("Plano de treinamento não encontrado.", 404);

        await EnsureNoActiveEnrollmentAsync(pokemonId, ct);
        EnsureMeetsEliteDosQuatroLevel(pokemon, plan);

        var enrollment = new Enrollment
        {
            PokemonId = pokemon.Id,
            TrainingPlanId = plan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = null,
            MonthlyPrice = plan.MonthlyPrice
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync(ct);

        return enrollment;
    }

    private async Task EnsureNoActiveEnrollmentAsync(int pokemonId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var hasActive = await _db.Enrollments.AnyAsync(
            e => e.PokemonId == pokemonId && (e.EndDate == null || e.EndDate.Value.Date >= today), ct);

        if (hasActive)
        {
            throw new DomainValidationException(
                "Este Pokémon já possui uma matrícula ativa.", 409);
        }
    }

    private static void EnsureMeetsEliteDosQuatroLevel(Pokemon pokemon, TrainingPlan plan)
    {
        if (plan.Name == EliteDosQuatroPlanName && pokemon.Level < EliteDosQuatroMinimumLevel)
        {
            throw new DomainValidationException(
                $"Nível mínimo de {EliteDosQuatroMinimumLevel} é necessário para o plano Elite dos 4.");
        }
    }

    /// <summary>Calculates (without side effects) the first-charge amount of an upgrade (FR-009).</summary>
    public async Task<ProrationResult> PreviewUpgradeAsync(int enrollmentId, int newTrainingPlanId, CancellationToken ct = default)
    {
        var (enrollment, newPlan, pokemon) = await LoadUpgradeContextAsync(enrollmentId, newTrainingPlanId, ct);
        return CalculateProration(enrollment, newPlan, pokemon);
    }

    /// <summary>Applies a previously calculated upgrade: closes the old enrollment and creates the new one (FR-010).</summary>
    public async Task<(Enrollment ClosedEnrollment, Enrollment NewEnrollment, ProrationResult Proration)> ConfirmUpgradeAsync(
        int enrollmentId, int newTrainingPlanId, CancellationToken ct = default)
    {
        var (enrollment, newPlan, pokemon) = await LoadUpgradeContextAsync(enrollmentId, newTrainingPlanId, ct);
        var proration = CalculateProration(enrollment, newPlan, pokemon);

        var upgradeDate = DateTime.UtcNow;
        enrollment.EndDate = upgradeDate;

        var newEnrollment = new Enrollment
        {
            PokemonId = enrollment.PokemonId,
            TrainingPlanId = newPlan.Id,
            StartDate = upgradeDate,
            EndDate = null,
            MonthlyPrice = newPlan.MonthlyPrice
        };

        _db.Enrollments.Add(newEnrollment);
        await _db.SaveChangesAsync(ct);

        return (enrollment, newEnrollment, proration);
    }

    private async Task<(Enrollment Enrollment, TrainingPlan NewPlan, Pokemon Pokemon)> LoadUpgradeContextAsync(
        int enrollmentId, int newTrainingPlanId, CancellationToken ct)
    {
        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, ct)
            ?? throw new DomainValidationException("Matrícula não encontrada.", 404);

        if (!enrollment.IsActive)
        {
            throw new DomainValidationException("Esta matrícula não está ativa.");
        }

        var newPlan = await _db.TrainingPlans.FirstOrDefaultAsync(p => p.Id == newTrainingPlanId, ct)
            ?? throw new DomainValidationException("Plano de treinamento não encontrado.", 404);

        var pokemon = await _db.Pokemons.FirstAsync(p => p.Id == enrollment.PokemonId, ct);

        if (newPlan.MonthlyPrice <= enrollment.MonthlyPrice)
        {
            throw new DomainValidationException("Downgrade de plano não é permitido.");
        }

        EnsureMeetsEliteDosQuatroLevel(pokemon, newPlan);

        return (enrollment, newPlan, pokemon);
    }

    private static ProrationResult CalculateProration(Enrollment enrollment, TrainingPlan newPlan, Pokemon _) =>
        BillingCycleCalculator.CalculateUpgradeProration(
            enrollment.StartDate, DateTime.UtcNow, enrollment.MonthlyPrice, newPlan.MonthlyPrice);

    /// <summary>Cancels an active enrollment: EndDate = end of the current paid cycle, no refund (FR-012, R4).</summary>
    public async Task<Enrollment> CancelEnrollmentAsync(int enrollmentId, CancellationToken ct = default)
    {
        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, ct)
            ?? throw new DomainValidationException("Matrícula não encontrada.", 404);

        if (!enrollment.IsActive)
        {
            throw new DomainValidationException("Esta matrícula já está encerrada.");
        }

        var (_, cycleEnd) = BillingCycleCalculator.GetCurrentCycle(enrollment.StartDate, DateTime.UtcNow);
        enrollment.EndDate = cycleEnd;

        await _db.SaveChangesAsync(ct);
        return enrollment;
    }

    /// <summary>
    /// Transfers a Pokémon to another Trainer (FR-015, R5). If it has an
    /// active enrollment, that enrollment is closed on the transfer date and
    /// an equivalent new enrollment is automatically created under the
    /// destination Trainer, billed in full (no proration — it's a new cycle).
    /// </summary>
    public async Task<(Pokemon Pokemon, Enrollment? ClosedEnrollment, Enrollment? NewEnrollment)> TransferPokemonAsync(
        int pokemonId, int newTrainerId, CancellationToken ct = default)
    {
        var pokemon = await _db.Pokemons.FirstOrDefaultAsync(p => p.Id == pokemonId, ct)
            ?? throw new DomainValidationException("Pokémon não encontrado.", 404);

        var newTrainerExists = await _db.Trainers.AnyAsync(t => t.Id == newTrainerId, ct);
        if (!newTrainerExists)
        {
            throw new DomainValidationException("Treinador de destino não encontrado.", 404);
        }

        if (pokemon.TrainerId == newTrainerId)
        {
            throw new DomainValidationException("O Pokémon já pertence a este Treinador.");
        }

        var today = DateTime.UtcNow.Date;
        // OrderByDescending(StartDate) é essencial aqui: no mesmo dia em que um
        // upgrade/transferência anterior encerrou uma matrícula (EndDate = hoje),
        // essa matrícula antiga ainda satisfaz a checagem de "ativa" por data
        // (FR-020) junto com a matrícula nova que a substituiu — sem essa
        // ordenação, a query poderia pegar a matrícula ERRADA (já superada) e
        // tentar reabri-la, violando o índice único (bug encontrado via teste
        // manual ponta a ponta, corrigido aqui).
        var activeEnrollment = await _db.Enrollments
            .Where(e => e.PokemonId == pokemonId && (e.EndDate == null || e.EndDate.Value.Date >= today))
            .OrderByDescending(e => e.StartDate)
            .FirstOrDefaultAsync(ct);

        Enrollment? closedEnrollment = null;
        Enrollment? newEnrollment = null;

        if (activeEnrollment is not null)
        {
            var transferDate = DateTime.UtcNow;
            activeEnrollment.EndDate = transferDate;
            closedEnrollment = activeEnrollment;

            newEnrollment = new Enrollment
            {
                PokemonId = pokemon.Id,
                TrainingPlanId = activeEnrollment.TrainingPlanId,
                StartDate = transferDate,
                EndDate = null,
                MonthlyPrice = activeEnrollment.MonthlyPrice
            };
            _db.Enrollments.Add(newEnrollment);
        }

        pokemon.TrainerId = newTrainerId;
        await _db.SaveChangesAsync(ct);

        return (pokemon, closedEnrollment, newEnrollment);
    }
}
