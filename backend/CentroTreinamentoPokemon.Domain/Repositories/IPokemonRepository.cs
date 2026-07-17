using CentroTreinamentoPokemon.Domain.Entities;

namespace CentroTreinamentoPokemon.Domain.Repositories;

public interface IPokemonRepository
{
    Task<Pokemon?> RecuperarPorIdAsync(int id);

    Task<IList<Pokemon>> ListarAsync();

    Task<IList<Pokemon>> ListarPorTreinadorAsync(
        int treinadorId);

    Task InserirAsync(Pokemon pokemon);
}