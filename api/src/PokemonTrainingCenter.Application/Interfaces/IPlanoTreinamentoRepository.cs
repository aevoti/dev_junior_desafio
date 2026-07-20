using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Application.Interfaces;

public interface IPlanoTreinamentoRepository
{
    Task<List<PlanoTreinamento>> ListarAsync();
    Task<PlanoTreinamento?> ObterPorIdAsync(int id);
}
