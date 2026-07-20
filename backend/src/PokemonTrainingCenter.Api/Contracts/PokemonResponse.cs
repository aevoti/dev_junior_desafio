namespace PokemonTrainingCenter.Api.Contracts;

public record PokemonResponse(int Id, string Name, string Type, int Level, int TrainerId);

public record CreatePokemonRequest(string Name, string Type, int Level, int TrainerId);
