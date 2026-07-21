using PokemonTrainingCenter.Domain.Services;

namespace PokemonTrainingCenter.Api.Contracts;

public record UpgradeRequest(int NewTrainingPlanId);

public record UpgradePreviewResponse(
    decimal CurrentPlanCredit,
    decimal NewPlanProratedCost,
    decimal FirstChargeAmount,
    DateTime CycleEndDate,
    int DaysRemainingInCycle)
{
    public static UpgradePreviewResponse From(ProrationResult result) => new(
        result.CurrentPlanCredit,
        result.NewPlanProratedCost,
        result.FirstChargeAmount,
        result.CycleEndDate,
        result.DaysRemainingInCycle);
}

public record UpgradeConfirmResponse(int ClosedEnrollmentId, EnrollmentResponse NewEnrollment, decimal FirstChargeAmount);
