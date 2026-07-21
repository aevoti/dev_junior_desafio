namespace PokemonTrainingCenter.Api.Contracts;

public record TransferPokemonRequest(int NewTrainerId);

public record TransferPokemonResponse(PokemonResponse Pokemon, int? ClosedEnrollmentId, int? NewEnrollmentId);
