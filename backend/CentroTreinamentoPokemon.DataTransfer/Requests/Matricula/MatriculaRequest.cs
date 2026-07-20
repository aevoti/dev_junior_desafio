namespace CentroTreinamentoPokemon.DataTransfer.Requests.Matricula;

public class MatriculaRequest
{
    public int PokemonId { get; set; }

    public int PlanoTreinamentoId { get; set; }

    public DateTime DataInicio { get; set; }

}