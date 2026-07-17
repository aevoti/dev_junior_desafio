using Microsoft.EntityFrameworkCore;
using PokemonTraining.Api.Data;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Entities;
using PokemonTraining.Api.Enums;
using PokemonTraining.Api.Exceptions;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Tests;

public class PokemonServiceTests
{
    [Fact]
    public async Task TransferirAsync_DeveAlterarTreinadorEPreservarTodosOsDadosDaMatricula()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context);
        var service = new PokemonService(context);

        var response = await service.TransferirAsync(
            cenario.Pokemon.Id,
            new TransferirPokemonRequest { NovoTreinadorId = cenario.NovoTreinador.Id });

        Assert.Equal(cenario.NovoTreinador.Id, response.TreinadorId);
        Assert.Equal(cenario.NovoTreinador.Nome, response.TreinadorNome);

        var matriculaPersistida = Assert.Single(context.Matriculas);
        Assert.Equal(cenario.Matricula.Id, matriculaPersistida.Id);
        Assert.Equal(cenario.Pokemon.Id, matriculaPersistida.PokemonId);
        Assert.Equal(StatusMatricula.Ativa, matriculaPersistida.Status);
        Assert.Equal(cenario.Plano.Id, matriculaPersistida.PlanoTreinamentoId);
        Assert.Equal(cenario.Matricula.ValorMensal, matriculaPersistida.ValorMensal);
        Assert.Equal(cenario.Matricula.DataInicio, matriculaPersistida.DataInicio);
        Assert.Equal(cenario.Matricula.DataFim, matriculaPersistida.DataFim);
        Assert.Equal(cenario.Matricula.MotivoEncerramento, matriculaPersistida.MotivoEncerramento);
    }

    [Fact]
    public async Task TransferirAsync_PokemonInexistente_DeveLancarRecursoNaoEncontradoException()
    {
        await using var context = TestDbContextFactory.Criar();
        var service = new PokemonService(context);

        var exception = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            service.TransferirAsync(999, new TransferirPokemonRequest { NovoTreinadorId = 1 }));

        Assert.Equal("Pokémon não encontrado.", exception.Message);
    }

    [Fact]
    public async Task TransferirAsync_TreinadorInexistente_DeveLancarRecursoNaoEncontradoException()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context);
        var service = new PokemonService(context);

        var exception = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            service.TransferirAsync(cenario.Pokemon.Id, new TransferirPokemonRequest { NovoTreinadorId = 999 }));

        Assert.Equal("Treinador não encontrado.", exception.Message);
    }

    [Fact]
    public async Task TransferirAsync_NaoDeveCriarNovaMatricula()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context);
        var matriculaIdOriginal = cenario.Matricula.Id;
        var service = new PokemonService(context);

        await service.TransferirAsync(
            cenario.Pokemon.Id,
            new TransferirPokemonRequest { NovoTreinadorId = cenario.NovoTreinador.Id });

        var matriculas = await context.Matriculas.ToListAsync();
        var matricula = Assert.Single(matriculas);
        Assert.Equal(matriculaIdOriginal, matricula.Id);
    }

    [Fact]
    public async Task TransferirAsync_NaoDeveAlterarQuantidadeDeMatriculasDoPokemon()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context);
        context.Matriculas.Add(new Matricula
        {
            PokemonId = cenario.Pokemon.Id,
            PlanoTreinamentoId = cenario.Plano.Id,
            DataInicio = new DateTime(2025, 12, 1, 12, 0, 0, DateTimeKind.Utc),
            DataFim = new DateTime(2025, 12, 31, 12, 0, 0, DateTimeKind.Utc),
            Status = StatusMatricula.Concluida,
            ValorMensal = cenario.Plano.ValorMensal,
            MotivoEncerramento = "Ciclo anterior concluído"
        });
        await context.SaveChangesAsync();
        var quantidadeAntes = await context.Matriculas.CountAsync(x => x.PokemonId == cenario.Pokemon.Id);
        var service = new PokemonService(context);

        await service.TransferirAsync(
            cenario.Pokemon.Id,
            new TransferirPokemonRequest { NovoTreinadorId = cenario.NovoTreinador.Id });

        var quantidadeDepois = await context.Matriculas.CountAsync(x => x.PokemonId == cenario.Pokemon.Id);
        Assert.Equal(2, quantidadeAntes);
        Assert.Equal(quantidadeAntes, quantidadeDepois);
    }

    private static async Task<CenarioTransferencia> CriarCenarioAsync(PokemonTrainingDbContext context)
    {
        var treinadorOriginal = CriarTreinador("Original");
        var novoTreinador = CriarTreinador("Novo");
        var plano = new PlanoTreinamento
        {
            Nome = "Ginásio Teste",
            ValorMensal = 50m,
            Descricao = "Plano de teste",
            Ordem = 1,
            NivelMinimo = 1
        };
        var pokemon = new Pokemon
        {
            Nome = "Bulbasaur Teste",
            Tipo = "Planta",
            Nivel = 20,
            Treinador = treinadorOriginal
        };
        var matricula = new Matricula
        {
            Pokemon = pokemon,
            PlanoTreinamento = plano,
            DataInicio = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc),
            DataFim = null,
            Status = StatusMatricula.Ativa,
            ValorMensal = plano.ValorMensal,
            MotivoEncerramento = null
        };
        context.AddRange(treinadorOriginal, novoTreinador, plano, pokemon, matricula);
        await context.SaveChangesAsync();
        return new CenarioTransferencia(treinadorOriginal, novoTreinador, pokemon, plano, matricula);
    }

    private static Treinador CriarTreinador(string sufixo) => new()
    {
        Nome = $"Treinador {sufixo}",
        Email = $"{sufixo.ToLowerInvariant()}-{Guid.NewGuid()}@example.com",
        CidadeOrigem = "Pallet"
    };

    private sealed record CenarioTransferencia(
        Treinador TreinadorOriginal,
        Treinador NovoTreinador,
        Pokemon Pokemon,
        PlanoTreinamento Plano,
        Matricula Matricula);
}
