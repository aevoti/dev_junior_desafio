using PokemonTraining.Api.Data;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Entities;
using PokemonTraining.Api.Enums;
using PokemonTraining.Api.Exceptions;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Tests;

public class UpgradeMatriculaServiceTests
{
    [Fact]
    public async Task SimularUpgradeAsync_DeveRejeitarDowngrade()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context, nivel: 60, ordemAtual: 2);
        var service = new MatriculaService(context);

        var action = () => service.SimularUpgradeAsync(
            cenario.Matricula.Id,
            new UpgradeMatriculaRequest { NovoPlanoTreinamentoId = cenario.PlanoInferior.Id });

        await Assert.ThrowsAsync<RegraNegocioException>(action);
    }

    [Fact]
    public async Task SimularUpgradeAsync_DeveRejeitarMesmoPlano()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context, nivel: 60, ordemAtual: 1);
        var service = new MatriculaService(context);

        var action = () => service.SimularUpgradeAsync(
            cenario.Matricula.Id,
            new UpgradeMatriculaRequest { NovoPlanoTreinamentoId = cenario.PlanoAtual.Id });

        await Assert.ThrowsAsync<RegraNegocioException>(action);
    }

    [Fact]
    public async Task SimularUpgradeAsync_DeveRejeitarNivelInsuficiente()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context, nivel: 49, ordemAtual: 1);
        var service = new MatriculaService(context);

        var action = () => service.SimularUpgradeAsync(
            cenario.Matricula.Id,
            new UpgradeMatriculaRequest { NovoPlanoTreinamentoId = cenario.PlanoElite.Id });

        await Assert.ThrowsAsync<RegraNegocioException>(action);
    }

    [Fact]
    public async Task SimularUpgradeAsync_NaoDeveAlterarBanco()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context, nivel: 60, ordemAtual: 1);
        var service = new MatriculaService(context);

        var response = await service.SimularUpgradeAsync(
            cenario.Matricula.Id,
            new UpgradeMatriculaRequest { NovoPlanoTreinamentoId = cenario.PlanoRegional.Id });

        Assert.True(response.PrimeiraCobranca > 0);
        Assert.Single(context.Matriculas);
        Assert.Equal(StatusMatricula.Ativa, cenario.Matricula.Status);
        Assert.Null(cenario.Matricula.DataFim);
    }

    [Fact]
    public async Task RealizarUpgradeAsync_DeveEncerrarAnteriorECriarNovaAtiva()
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context, nivel: 60, ordemAtual: 1);
        var service = new MatriculaService(context);

        var response = await service.RealizarUpgradeAsync(
            cenario.Matricula.Id,
            new UpgradeMatriculaRequest { NovoPlanoTreinamentoId = cenario.PlanoRegional.Id });

        Assert.Equal(2, context.Matriculas.Count());
        Assert.Equal(StatusMatricula.Concluida, cenario.Matricula.Status);
        Assert.NotNull(cenario.Matricula.DataFim);
        Assert.Equal("Upgrade para o plano Liga Regional Teste.", cenario.Matricula.MotivoEncerramento);
        var nova = context.Matriculas.Single(x => x.Id == response.NovaMatriculaId);
        Assert.Equal(StatusMatricula.Ativa, nova.Status);
        Assert.Equal(cenario.PlanoRegional.ValorMensal, nova.ValorMensal);
        Assert.Single(context.Matriculas.Where(x => x.Status == StatusMatricula.Ativa));
    }

    [Theory]
    [InlineData(StatusMatricula.Cancelada)]
    [InlineData(StatusMatricula.Concluida)]
    public async Task SimularUpgradeAsync_DeveRejeitarMatriculaInativa(StatusMatricula status)
    {
        await using var context = TestDbContextFactory.Criar();
        var cenario = await CriarCenarioAsync(context, nivel: 60, ordemAtual: 1, status);
        var service = new MatriculaService(context);

        var action = () => service.SimularUpgradeAsync(
            cenario.Matricula.Id,
            new UpgradeMatriculaRequest { NovoPlanoTreinamentoId = cenario.PlanoRegional.Id });

        var exception = await Assert.ThrowsAsync<RegraNegocioException>(action);
        Assert.Equal("Somente matrículas ativas podem receber upgrade.", exception.Message);
    }

    private static async Task<CenarioUpgrade> CriarCenarioAsync(
        PokemonTrainingDbContext context,
        int nivel,
        int ordemAtual,
        StatusMatricula status = StatusMatricula.Ativa)
    {
        var treinador = new Treinador
        {
            Nome = "Treinadora Upgrade",
            Email = $"upgrade-{Guid.NewGuid()}@example.com",
            CidadeOrigem = "Pallet"
        };
        var pokemon = new Pokemon
        {
            Nome = "Pikachu Upgrade",
            Tipo = "Elétrico",
            Nivel = nivel,
            Treinador = treinador
        };
        var local = CriarPlano("Ginásio Local Teste", 50m, 1, 1);
        var regional = CriarPlano("Liga Regional Teste", 120m, 2, 1);
        var elite = CriarPlano("Elite dos 4 Teste", 300m, 3, 50);
        var planoAtual = ordemAtual == 1 ? local : regional;
        var matricula = new Matricula
        {
            Pokemon = pokemon,
            PlanoTreinamento = planoAtual,
            DataInicio = DateTime.UtcNow.Date.AddDays(-15),
            DataFim = status == StatusMatricula.Ativa ? null : DateTime.UtcNow,
            Status = status,
            ValorMensal = planoAtual.ValorMensal
        };

        context.AddRange(treinador, pokemon, local, regional, elite, matricula);
        await context.SaveChangesAsync();
        return new CenarioUpgrade(matricula, planoAtual, local, regional, elite);
    }

    private static PlanoTreinamento CriarPlano(
        string nome,
        decimal valor,
        int ordem,
        int nivelMinimo) => new()
    {
        Nome = nome,
        ValorMensal = valor,
        Descricao = "Plano para teste de upgrade",
        Ordem = ordem,
        NivelMinimo = nivelMinimo
    };

    private sealed record CenarioUpgrade(
        Matricula Matricula,
        PlanoTreinamento PlanoAtual,
        PlanoTreinamento PlanoInferior,
        PlanoTreinamento PlanoRegional,
        PlanoTreinamento PlanoElite);
}
