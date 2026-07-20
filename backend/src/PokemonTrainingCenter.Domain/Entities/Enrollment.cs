namespace PokemonTrainingCenter.Domain.Entities;

/// <summary>
/// Links a Pokémon to a TrainingPlan for a period of time. Status is never
/// persisted directly — it is always derived from <see cref="EndDate"/>
/// (FR-020): null or today-or-future means active, past means ended.
/// </summary>
public class Enrollment
{
    public int Id { get; set; }

    public int PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }

    public int TrainingPlanId { get; set; }
    public TrainingPlan? TrainingPlan { get; set; }

    /// <summary>
    /// Snapshot of the Trainer who owned the Pokémon when this enrollment
    /// was created (FR-027). Unlike <see cref="Entities.Pokemon.TrainerId"/>,
    /// this never changes — a later transfer (R5) updates the Pokémon's
    /// current owner but must not rewrite the owner recorded on existing
    /// enrollments, active or ended.
    /// </summary>
    public int TrainerId { get; set; }
    public Trainer? Trainer { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Snapshot of TrainingPlan.MonthlyPrice at enrollment time.</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// True when EndDate is absent or is a future/current instant (FR-020,
    /// R1). Compared by exact instant, not by date: cancellation
    /// (CancelEnrollmentAsync) stores the end of the UTC calendar day so the
    /// Pokémon keeps access through that whole day, while upgrade/transfer
    /// store the exact operation instant so the closed enrollment stops
    /// counting as active immediately, even on the same calendar day.
    /// </summary>
    public bool IsActive => EndDate is null || EndDate.Value >= DateTime.UtcNow;
}
