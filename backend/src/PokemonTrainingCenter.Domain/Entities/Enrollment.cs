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

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Snapshot of TrainingPlan.MonthlyPrice at enrollment time.</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// True when EndDate is absent or today/future (FR-020, R1). Compared by
    /// date (not by instant) because the spec's clarification explicitly
    /// defines this granularity. Known limitation: if an enrollment is
    /// closed by an upgrade/transfer and another operation happens on the
    /// same calendar day, the old enrollment still shows as "active" until
    /// the day rolls over — the filtered unique index (EndDate IS NULL)
    /// still guarantees that two *open* enrollments (no end date) can never
    /// coexist.
    /// </summary>
    public bool IsActive => EndDate is null || EndDate.Value.Date >= DateTime.UtcNow.Date;
}
