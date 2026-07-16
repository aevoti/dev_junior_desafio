using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Entities;
using PokemonTraining.Api.Enums;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Tests;

public class PokemonServiceTests
{
    [Fact]
    public async Task TransferirAsync_DeveAlterarTreinadorEPreservarMatricula()
    {
        await using var context = TestDbContextFactory.Criar();
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
            DataInicio = DateTime.UtcNow,
            Status = StatusMatricula.Ativa,
            ValorMensal = plano.ValorMensal
        };
        context.AddRange(treinadorOriginal, novoTreinador, plano, pokemon, matricula);
        await context.SaveChangesAsync();
        var service = new PokemonService(context);

        var response = await service.TransferirAsync(
            pokemon.Id,
            new TransferirPokemonRequest { NovoTreinadorId = novoTreinador.Id });

        Assert.Equal(novoTreinador.Id, response.TreinadorId);
        var matriculaPersistida = Assert.Single(context.Matriculas);
        Assert.Equal(StatusMatricula.Ativa, matriculaPersistida.Status);
        Assert.Equal(plano.Id, matriculaPersistida.PlanoTreinamentoId);
        Assert.Equal(pokemon.Id, matriculaPersistida.PokemonId);
    }

    private static Treinador CriarTreinador(string sufixo) => new()
    {
        Nome = $"Treinador {sufixo}",
        Email = $"{sufixo.ToLowerInvariant()}-{Guid.NewGuid()}@example.com",
        CidadeOrigem = "Pallet"
    };
}
