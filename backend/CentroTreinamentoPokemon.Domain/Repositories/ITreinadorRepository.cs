using CentroTreinamentoPokemon.Domain.Entities;

namespace CentroTreinamentoPokemon.Domain.Repositories;

public interface ITreinadorRepository
{
    Task<Treinador?> RecuperarPorIdAsync(int id);

    Task<Treinador?> RecuperarPorEmailAsync(string email);

    Task<IList<Treinador>> ListarAsync();

    Task InserirAsync(Treinador treinador);

    Task<bool> ExistePorEmailAsync(string email);
}