using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Data;
using PokemonTraining.Api.Entities;
using PokemonTraining.Api.Enums;
using PokemonTraining.Api.Exceptions;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Tests;

public class MatriculaServiceTests
{
    [Fact]
    public async Task CriarAsync_DeveRejeitarSegundaMatriculaAtiva()
    {
        await using var context = TestDbContextFactory.Criar();
        var (pokemon, plano) = await CriarCenarioAsync(context, nivel: 30, nivelMinimo: 1);
        context.Matriculas.Add(CriarMatricula(pokemon, plano, StatusMatricula.Ativa));
        await context.SaveChangesAsync();
        var service = new MatriculaService(context);

        var action = () => service.CriarAsync(NovaMatricula(pokemon.Id, plano.Id));

        var exception = await Assert.ThrowsAsync<ConflitoException>(action);
        Assert.Equal("Este Pokémon já possui uma matrícula ativa.", exception.Message);
    }

    [Fact]
    public async Task CriarAsync_DevePermitirMatriculaDepoisDeCancelamento()
    {
        await using var context = TestDbContextFactory.Criar();
        var (pokemon, plano) = await CriarCenarioAsync(context, nivel: 30, nivelMinimo: 1);
        context.Matriculas.Add(CriarMatricula(pokemon, plano, StatusMatricula.Cancelada));
        await context.SaveChangesAsync();
        var service = new MatriculaService(context);

        var response = await service.CriarAsync(NovaMatricula(pokemon.Id, plano.Id));

        Assert.Equal(StatusMatricula.Ativa, response.Status);
        Assert.Equal(2, context.Matriculas.Count());
    }

    [Fact]
    public async Task CriarAsync_DeveRejeitarNivelAbaixoDoMinimo()
    {
        await using var context = TestDbContextFactory.Criar();
        var (pokemon, plano) = await CriarCenarioAsync(context, nivel: 49, nivelMinimo: 50);
        var service = new MatriculaService(context);

        var action = () => service.CriarAsync(NovaMatricula(pokemon.Id, plano.Id));

        var exception = await Assert.ThrowsAsync<RegraNegocioException>(action);
        Assert.Equal("O Pokémon não possui o nível mínimo exigido para este plano.", exception.Message);
    }

    [Fact]
    public async Task CriarAsync_DevePermitirNivelMinimoExato()
    {
        await using var context = TestDbContextFactory.Criar();
        var (pokemon, plano) = await CriarCenarioAsync(context, nivel: 50, nivelMinimo: 50);
        var service = new MatriculaService(context);

        var response = await service.CriarAsync(NovaMatricula(pokemon.Id, plano.Id));

        Assert.Equal(StatusMatricula.Ativa, response.Status);
        Assert.Equal(plano.ValorMensal, response.ValorMensal);
    }

    [Fact]
    public async Task CancelarAsync_DeveCancelarSemExcluirHistorico()
    {
        await using var context = TestDbContextFactory.Criar();
        var (pokemon, plano) = await CriarCenarioAsync(context, nivel: 30, nivelMinimo: 1);
        var matricula = CriarMatricula(pokemon, plano, StatusMatricula.Ativa);
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();
        var service = new MatriculaService(context);

        var response = await service.CancelarAsync(
            matricula.Id,
            new CancelarMatriculaRequest { Motivo = "Mudança de rotina" });

        Assert.Equal(StatusMatricula.Cancelada, response.Status);
        Assert.NotNull(response.DataFim);
        Assert.Equal("Mudança de rotina", response.MotivoEncerramento);
        Assert.Single(context.Matriculas);
    }

    [Fact]
    public async Task CancelarAsync_DeveRejeitarMatriculaJaCancelada()
    {
        await using var context = TestDbContextFactory.Criar();
        var (pokemon, plano) = await CriarCenarioAsync(context, nivel: 30, nivelMinimo: 1);
        var matricula = CriarMatricula(pokemon, plano, StatusMatricula.Cancelada);
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();
        var service = new MatriculaService(context);

        var action = () => service.CancelarAsync(matricula.Id, new CancelarMatriculaRequest());

        var exception = await Assert.ThrowsAsync<RegraNegocioException>(action);
        Assert.Equal("Somente matrículas ativas podem ser canceladas.", exception.Message);
    }

    private static async Task<(Pokemon Pokemon, PlanoTreinamento Plano)> CriarCenarioAsync(
        PokemonTrainingDbContext context,
        int nivel,
        int nivelMinimo)
    {
        var treinador = new Treinador
        {
            Nome = "Treinadora Teste",
            Email = $"teste-{Guid.NewGuid()}@example.com",
            CidadeOrigem = "Pallet"
        };
        var pokemon = new Pokemon
        {
            Nome = "Pikachu Teste",
            Tipo = "Elétrico",
            Nivel = nivel,
            Treinador = treinador
        };
        var plano = new PlanoTreinamento
        {
            Nome = $"Plano {Guid.NewGuid()}",
            ValorMensal = 300m,
            Descricao = "Plano para testes",
            Ordem = 1,
            NivelMinimo = nivelMinimo
        };

        context.AddRange(treinador, pokemon, plano);
        await context.SaveChangesAsync();
        return (pokemon, plano);
    }

    private static Matricula CriarMatricula(
        Pokemon pokemon,
        PlanoTreinamento plano,
        StatusMatricula status) => new()
        {
            Pokemon = pokemon,
            PlanoTreinamento = plano,
            DataInicio = DateTime.UtcNow.AddDays(-1),
            DataFim = status == StatusMatricula.Ativa ? null : DateTime.UtcNow,
            Status = status,
            ValorMensal = plano.ValorMensal
        };

    private static CriarMatriculaRequest NovaMatricula(int pokemonId, int planoId) => new()
    {
        PokemonId = pokemonId,
        PlanoTreinamentoId = planoId,
        DataInicio = DateTime.UtcNow
    };
}
