using CentroTreinamentoPokemon.Domain.Entities;

namespace CentroTreinamentoPokemon.Domain.Repositories;

public interface IPlanoTreinamentoRepository
{
    Task<PlanoTreinamento?> RecuperarPorIdAsync(int id);

    Task<IList<PlanoTreinamento>> ListarAsync();
}