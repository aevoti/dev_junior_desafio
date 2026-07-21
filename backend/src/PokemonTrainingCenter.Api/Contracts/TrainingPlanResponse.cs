namespace PokemonTrainingCenter.Api.Contracts;

public record TrainingPlanResponse(int Id, string Name, decimal MonthlyPrice, string Description);
