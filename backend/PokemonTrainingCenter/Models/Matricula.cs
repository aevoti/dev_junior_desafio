namespace PokemonTrainingCenter.Models;

public class Matricula
{
    public int Id { get; set; }
    public int PokemonId { get; set; }
    public PlanoTreinamento Plano { get; set; }
    public DateTime DataInicio { get; set; }
    public StatusMatricula Status { get; set; }
    public decimal ValorMensal { get; set; }

    public Pokemon Pokemon { get; set; } = null!;

    public static decimal ObterValorPlano(PlanoTreinamento plano) => plano switch
    {
        PlanoTreinamento.GinasioLocal => 50m,
        PlanoTreinamento.LigaRegional => 120m,
        PlanoTreinamento.EliteDos4 => 300m,
        _ => throw new ArgumentException("Plano inválido")
    };
}
