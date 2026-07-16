using PokemonTraining.Api.Exceptions;
using PokemonTraining.Api.Services;

namespace PokemonTraining.Tests;

public class CalculadoraProRataTests
{
    [Fact]
    public void Calcular_DeveReproduzirExemploObrigatorio()
    {
        var inicio = new DateTime(2026, 1, 1);

        var resultado = CalculadoraProRata.Calcular(inicio, inicio.AddDays(15), 50m, 120m);

        Assert.Equal(15, resultado.DiasRestantes);
        Assert.Equal(25m, resultado.CreditoPlanoAnterior);
        Assert.Equal(60m, resultado.CustoProporcionalNovoPlano);
        Assert.Equal(35m, resultado.PrimeiraCobranca);
    }

    [Fact]
    public void Calcular_DeveConsiderarTrintaDiasNoMesmoDia()
    {
        var inicio = new DateTime(2026, 1, 1);

        var resultado = CalculadoraProRata.Calcular(inicio, inicio, 50m, 120m);

        Assert.Equal(30, resultado.DiasRestantes);
    }

    [Fact]
    public void Calcular_DeveIniciarNovoCicloDepoisDeTrintaDias()
    {
        var inicio = new DateTime(2026, 1, 1);

        var resultado = CalculadoraProRata.Calcular(inicio, inicio.AddDays(30), 50m, 120m);

        Assert.Equal(30, resultado.DiasRestantes);
    }

    [Fact]
    public void Calcular_DeveCalcularSegundoCiclo()
    {
        var inicio = new DateTime(2026, 1, 1);

        var resultado = CalculadoraProRata.Calcular(inicio, inicio.AddDays(45), 50m, 120m);

        Assert.Equal(15, resultado.DiasRestantes);
        Assert.Equal(35m, resultado.PrimeiraCobranca);
    }

    [Fact]
    public void Calcular_DeveRejeitarDataAnteriorAoInicio()
    {
        var inicio = new DateTime(2026, 1, 2);

        var action = () => CalculadoraProRata.Calcular(inicio, inicio.AddDays(-1), 50m, 120m);

        var exception = Assert.Throws<RegraNegocioException>(action);
        Assert.Equal("A data do upgrade não pode ser anterior ao início da matrícula.", exception.Message);
    }
}
