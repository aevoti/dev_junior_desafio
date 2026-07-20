using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Application.Interfaces;

public interface IMatriculaRepository
{
    Task<List<Matricula>> ListarAsync(string? nomeBusca, StatusMatricula? status);
    Task<Matricula?> ObterPorIdAsync(int id);
    Task<Matricula?> ObterAtivaPorPokemonAsync(int pokemonId);
    Task AdicionarAsync(Matricula matricula);
    Task AtualizarAsync(Matricula matricula);
}
