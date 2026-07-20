namespace CentroTreinamentoPokemon.DataTransfer.Responses.Matricula;

public class UpgradeMatriculaResponse
{
    public int MatriculaAtualId { get; set; }

    public int PlanoAtualId { get; set; }

    public string PlanoAtualNome { get; set; } = string.Empty;

    public int NovoPlanoId { get; set; }

    public string NovoPlanoNome { get; set; } = string.Empty;

    public int DiasRestantes { get; set; }

    public decimal CreditoPlanoAtual { get; set; }

    public decimal CustoProporcionalNovoPlano { get; set; }

    public decimal ValorPrimeiraCobranca { get; set; }

    public bool UpgradeConfirmado { get; set; }

    public int? NovaMatriculaId { get; set; }
}