using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Application.Interfaces;

public interface ITreinadorRepository
{
    Task<List<Treinador>> ListarAsync();
    Task<Treinador?> ObterPorIdAsync(int id);
    Task<Treinador?> ObterPorEmailAsync(string email);
    Task AdicionarAsync(Treinador treinador);
}
