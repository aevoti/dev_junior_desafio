using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Api.Contracts;

public record EnrollmentListItemResponse(
    int Id,
    string PokemonName,
    string TrainerName,
    string TrainingPlanName,
    DateTime StartDate,
    DateTime? EndDate,
    decimal MonthlyPrice,
    string Status)
{
    public static EnrollmentListItemResponse From(Enrollment enrollment) => new(
        enrollment.Id,
        enrollment.Pokemon!.Name,
        enrollment.Pokemon!.Trainer!.Name,
        enrollment.TrainingPlan!.Name,
        enrollment.StartDate,
        enrollment.EndDate,
        enrollment.MonthlyPrice,
        EnrollmentResponse.ResolveStatus(enrollment));
}
