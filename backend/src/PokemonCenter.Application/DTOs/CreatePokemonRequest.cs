namespace PokemonCenter.Application.DTOs;

public class CreatePokemonRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public int TreinadorId { get; set; }
}