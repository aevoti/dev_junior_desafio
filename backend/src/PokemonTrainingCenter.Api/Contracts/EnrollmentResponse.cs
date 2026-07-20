using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Api.Contracts;

public record EnrollmentResponse(
    int Id,
    int PokemonId,
    int TrainingPlanId,
    DateTime StartDate,
    DateTime? EndDate,
    decimal MonthlyPrice,
    string Status)
{
    /// <summary>Deriva o status em inglês (Active/EndingSoon/Ended — FR-020) a partir de EndDate.</summary>
    public static string ResolveStatus(Enrollment enrollment)
    {
        if (enrollment.EndDate is null)
        {
            return "Active";
        }

        return enrollment.EndDate.Value.Date >= DateTime.UtcNow.Date ? "EndingSoon" : "Ended";
    }

    public static EnrollmentResponse From(Enrollment enrollment) => new(
        enrollment.Id,
        enrollment.PokemonId,
        enrollment.TrainingPlanId,
        enrollment.StartDate,
        enrollment.EndDate,
        enrollment.MonthlyPrice,
        ResolveStatus(enrollment));
}

public record CreateEnrollmentRequest(int PokemonId, int TrainingPlanId);
