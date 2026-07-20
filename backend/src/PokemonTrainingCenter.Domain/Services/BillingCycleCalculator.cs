namespace PokemonTrainingCenter.Domain.Services;

public record ProrationResult(
    decimal CurrentPlanCredit,
    decimal NewPlanProratedCost,
    decimal FirstChargeAmount,
    DateTime CycleEndDate,
    int DaysRemainingInCycle);

/// <summary>
/// Calculates the monthly cycle boundaries and the pro-rata upgrade amount
/// (R2, FR-009, FR-021). A cycle always ends on the same date one month
/// after the enrollment's start date (or its last "monthly anniversary");
/// <see cref="DateTime.AddMonths"/> already clamps to the last day of the
/// month when the start day doesn't exist in the following month (e.g., an
/// enrollment started on 31/01 has a cycle ending on 28/02 or 29/02) — see
/// research.md item 4.
/// </summary>
public static class BillingCycleCalculator
{
    /// <summary>
    /// Finds the monthly cycle (start/end) that contains
    /// <paramref name="referenceDate"/>, anchored on the day of
    /// <paramref name="enrollmentStartDate"/>. Each boundary is recomputed
    /// from the original start date (not chained), so the anchor day isn't
    /// lost after an end-of-month clamp.
    /// </summary>
    public static (DateTime CycleStart, DateTime CycleEnd) GetCurrentCycle(DateTime enrollmentStartDate, DateTime referenceDate)
    {
        var monthsElapsed = ((referenceDate.Year - enrollmentStartDate.Year) * 12) + referenceDate.Month - enrollmentStartDate.Month;

        var candidateStart = enrollmentStartDate.AddMonths(monthsElapsed);
        if (candidateStart > referenceDate)
        {
            monthsElapsed--;
            candidateStart = enrollmentStartDate.AddMonths(monthsElapsed);
        }

        var cycleEnd = enrollmentStartDate.AddMonths(monthsElapsed + 1);
        return (candidateStart, cycleEnd);
    }

    /// <summary>Calculates the old plan's credit, the new plan's prorated cost, and the first charge (FR-009).</summary>
    public static ProrationResult CalculateUpgradeProration(
        DateTime enrollmentStartDate,
        DateTime upgradeDate,
        decimal currentPlanMonthlyPrice,
        decimal newPlanMonthlyPrice)
    {
        var (cycleStart, cycleEnd) = GetCurrentCycle(enrollmentStartDate, upgradeDate);
        var cycleDurationDays = (cycleEnd - cycleStart).Days;
        var daysRemaining = (cycleEnd - upgradeDate).Days;

        var credit = Round(currentPlanMonthlyPrice * daysRemaining / cycleDurationDays);
        var newProratedCost = Round(newPlanMonthlyPrice * daysRemaining / cycleDurationDays);
        var firstCharge = Round(newProratedCost - credit);

        return new ProrationResult(credit, newProratedCost, firstCharge, cycleEnd, daysRemaining);
    }

    /// <summary>Standard rounding to two decimal places (0.5 rounds up) — R2 clarification.</summary>
    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
