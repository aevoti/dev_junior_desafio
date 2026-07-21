using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Domain.Repositories;

public interface IPokemonRepository
{
    Task<Pokemon?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Pokemon>> GetAllWithTrainerAsync(CancellationToken ct = default);
    void Add(Pokemon pokemon);
}
