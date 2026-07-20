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
            // FR-027: snapshot do dono atual do Pokémon — nunca reescrito depois.
            TrainerId = pokemon.TrainerId,
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
        var now = DateTime.UtcNow;
        var hasActive = await _db.Enrollments.AnyAsync(
            e => e.PokemonId == pokemonId && (e.EndDate == null || e.EndDate.Value >= now), ct);

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
            // FR-027: dono não muda em um upgrade — mesmo TrainerId da matrícula anterior.
            TrainerId = enrollment.TrainerId,
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
        // FR-012/FR-020: grava o fim do dia (UTC), não o instante exato do
        // ciclo, para o Pokémon manter acesso durante todo esse dia.
        enrollment.EndDate = cycleEnd.Date.AddDays(1).AddTicks(-1);

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

        var newTrainer = await _db.Trainers.FirstOrDefaultAsync(t => t.Id == newTrainerId, ct)
            ?? throw new DomainValidationException("Treinador de destino não encontrado.", 404);

        if (pokemon.TrainerId == newTrainerId)
        {
            throw new DomainValidationException("O Pokémon já pertence a este Treinador.");
        }

        var now = DateTime.UtcNow;
        // FR-020: comparação por instante exato (não por data) — uma
        // matrícula encerrada por upgrade/transferência anterior no mesmo
        // dia já tem EndDate no passado em relação a "now", então não é mais
        // ambígua com a matrícula que a substituiu.
        var activeEnrollment = await _db.Enrollments
            .Where(e => e.PokemonId == pokemonId && (e.EndDate == null || e.EndDate.Value >= now))
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
                // FR-027: a matrícula NOVA passa a pertencer ao Treinador de
                // destino; a matrícula FECHADA (activeEnrollment) mantém seu
                // TrainerId original — nunca é reescrito aqui.
                TrainerId = newTrainerId,
                StartDate = transferDate,
                EndDate = null,
                MonthlyPrice = activeEnrollment.MonthlyPrice
            };
            _db.Enrollments.Add(newEnrollment);
        }

        pokemon.TrainerId = newTrainerId;
        pokemon.Trainer = newTrainer;
        await _db.SaveChangesAsync(ct);

        return (pokemon, closedEnrollment, newEnrollment);
    }
}
