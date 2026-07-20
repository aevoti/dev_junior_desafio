using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Enums;

namespace CentroTreinamentoPokemon.Domain.Repositories;

public interface IMatriculaRepository
{
    Task<Matricula?> RecuperarPorIdAsync(int id);

    Task<Matricula?> RecuperarAtivaPorPokemonIdAsync(
        int pokemonId);

    Task<bool> ExisteMatriculaAtivaPorPokemonIdAsync(
        int pokemonId);

    Task<IList<Matricula>> ListarAsync(
        string? busca,
        StatusMatriculaEnum? status);

    Task InserirAsync(Matricula matricula);
}