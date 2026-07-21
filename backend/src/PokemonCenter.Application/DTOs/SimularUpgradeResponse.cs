namespace PokemonCenter.Application.DTOs;

public class SimularUpgradeResponse
{
    public decimal ValorPrimeiraCobranca { get; set; }
    public int DiasRestantes { get; set; }
    public decimal CreditoPlanoAntigo { get; set; }
    public decimal CustoNovoPlano { get; set; }
}