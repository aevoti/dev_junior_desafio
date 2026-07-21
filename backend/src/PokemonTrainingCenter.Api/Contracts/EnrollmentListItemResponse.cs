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
        // FR-027: Treinador associado à matrícula (snapshot no momento da
        // criação), não o dono atual do Pokémon — ver Enrollment.TrainerId.
        enrollment.Trainer!.Name,
        enrollment.TrainingPlan!.Name,
        enrollment.StartDate,
        enrollment.EndDate,
        enrollment.MonthlyPrice,
        EnrollmentResponse.ResolveStatus(enrollment));
}
