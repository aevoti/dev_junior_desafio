namespace PokemonCenter.Application.DTOs;

public class CreateMatriculaRequest
{
    public int PokemonId { get; set; }
    public int PlanoId { get; set; }
    public DateTime DataInicio { get; set; }
}