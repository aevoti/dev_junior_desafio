using PokemonTraining.Api.Exceptions;

namespace PokemonTraining.Api.Services;

public record ResultadoProRata(
    int DiasRestantes,
    decimal CreditoPlanoAnterior,
    decimal CustoProporcionalNovoPlano,
    decimal PrimeiraCobranca);

public static class CalculadoraProRata
{
    private const int DiasPorCiclo = 30;

    public static ResultadoProRata Calcular(
        DateTime dataInicio,
        DateTime dataUpgrade,
        decimal valorPlanoAnterior,
        decimal valorNovoPlano)
    {
        var diasDecorridosTotais = (dataUpgrade.Date - dataInicio.Date).Days;
        if (diasDecorridosTotais < 0)
        {
            throw new RegraNegocioException(
                "A data do upgrade não pode ser anterior ao início da matrícula.");
        }

        var diasDecorridosNoCiclo = diasDecorridosTotais % DiasPorCiclo;
        var diasRestantes = DiasPorCiclo - diasDecorridosNoCiclo;
        var creditoPlanoAnterior = Arredondar(valorPlanoAnterior * diasRestantes / DiasPorCiclo);
        var custoProporcionalNovoPlano = Arredondar(valorNovoPlano * diasRestantes / DiasPorCiclo);
        var primeiraCobranca = Arredondar(Math.Max(
            0m,
            custoProporcionalNovoPlano - creditoPlanoAnterior));

        return new ResultadoProRata(
            diasRestantes,
            creditoPlanoAnterior,
            custoProporcionalNovoPlano,
            primeiraCobranca);
    }

    private static decimal Arredondar(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
