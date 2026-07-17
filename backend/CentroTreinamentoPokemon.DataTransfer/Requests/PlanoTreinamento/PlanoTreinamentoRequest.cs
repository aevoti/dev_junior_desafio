namespace CentroTreinamentoPokemon.DataTransfer.Requests.PlanoTreinamento;

public class PlanoTreinamentoRequest
{
    public string Nome { get; set; } = string.Empty;

    public decimal ValorMensal { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public int NivelPlano { get; set; }
}