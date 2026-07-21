namespace PokemonTrainingCenter.Api.Contracts;

public record TrainerResponse(int Id, string Name, string Email, string City);

public record CreateTrainerRequest(string Name, string Email, string City);
