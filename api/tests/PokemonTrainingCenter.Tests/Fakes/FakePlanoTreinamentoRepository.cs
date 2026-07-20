using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Tests.Fakes;

public class FakePlanoTreinamentoRepository : IPlanoTreinamentoRepository
{
    public List<PlanoTreinamento> Dados { get; } = new();

    public Task<List<PlanoTreinamento>> ListarAsync() =>
        Task.FromResult(Dados.OrderBy(p => p.Nivel).ToList());

    public Task<PlanoTreinamento?> ObterPorIdAsync(int id) =>
        Task.FromResult(Dados.FirstOrDefault(p => p.Id == id));
}
