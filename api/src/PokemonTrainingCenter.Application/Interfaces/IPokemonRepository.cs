using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Application.Interfaces;

public interface IPokemonRepository
{
    Task<List<Pokemon>> ListarAsync();
    Task<Pokemon?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Pokemon pokemon);
    Task AtualizarAsync(Pokemon pokemon);
}
