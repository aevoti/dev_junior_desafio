namespace CentroTreinamentoPokemon.DataTransfer.Responses.Treinador;

public class TreinadorResponse
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string CidadeOrigem { get; set; } = string.Empty;
}