using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Tests.Fakes;

public class FakePokemonRepository : IPokemonRepository
{
    public List<Pokemon> Dados { get; } = new();

    public Task<List<Pokemon>> ListarAsync() => Task.FromResult(Dados.ToList());

    public Task<Pokemon?> ObterPorIdAsync(int id) =>
        Task.FromResult(Dados.FirstOrDefault(p => p.Id == id));

    public Task AdicionarAsync(Pokemon pokemon)
    {
        Dados.Add(pokemon);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Pokemon pokemon) => Task.CompletedTask;
}
