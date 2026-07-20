using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;

namespace PokemonTrainingCenter.Tests.Fakes;

/// <summary>
/// Repositório em memória, só para os testes de unidade do MatriculaService.
/// Recebe as listas de Planos/Pokémon pra simular o ".Include()" que o
/// repositório real (EF Core) faz ao carregar as navegações de uma Matrícula.
/// </summary>
public class FakeMatriculaRepository : IMatriculaRepository
{
    private readonly List<PlanoTreinamento> _planos;
    private readonly List<Pokemon> _pokemons;
    private int _proximoId = 1;

    public List<Matricula> Dados { get; } = new();

    public FakeMatriculaRepository(List<PlanoTreinamento> planos, List<Pokemon> pokemons)
    {
        _planos = planos;
        _pokemons = pokemons;
    }

    public Task<List<Matricula>> ListarAsync(string? nomeBusca, StatusMatricula? status)
    {
        var query = Dados.AsEnumerable();
        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }
        var resultado = query.ToList();
        resultado.ForEach(PreencherNavegacoes);
        return Task.FromResult(resultado);
    }

    public Task<Matricula?> ObterPorIdAsync(int id)
    {
        var matricula = Dados.FirstOrDefault(m => m.Id == id);
        if (matricula is not null)
        {
            PreencherNavegacoes(matricula);
        }
        return Task.FromResult(matricula);
    }

    public Task<Matricula?> ObterAtivaPorPokemonAsync(int pokemonId)
    {
        var matricula = Dados.FirstOrDefault(m => m.PokemonId == pokemonId && m.Status == StatusMatricula.Ativa);
        if (matricula is not null)
        {
            PreencherNavegacoes(matricula);
        }
        return Task.FromResult(matricula);
    }

    public Task AdicionarAsync(Matricula matricula)
    {
        matricula.Id = _proximoId++;
        Dados.Add(matricula);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Matricula matricula) => Task.CompletedTask;

    private void PreencherNavegacoes(Matricula matricula)
    {
        matricula.PlanoTreinamento ??= _planos.FirstOrDefault(p => p.Id == matricula.PlanoTreinamentoId);
        matricula.Pokemon ??= _pokemons.FirstOrDefault(p => p.Id == matricula.PokemonId);
    }
}
