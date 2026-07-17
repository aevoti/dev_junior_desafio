namespace CentroTreinamentoPokemon.DataTransfer.Requests.Matricula;

public class UpgradeMatriculaRequest
{
    public int NovoPlanoTreinamentoId { get; set; }

    public DateTime DataUpgrade { get; set; }
}