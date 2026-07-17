namespace CentroTreinamentoPokemon.DataTransfer.Responses.PlanoTreinamento;

public class PlanoTreinamentoResponse
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal ValorMensal { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public int NivelPlano { get; set; }
}