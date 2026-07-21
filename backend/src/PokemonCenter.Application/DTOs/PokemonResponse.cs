namespace PokemonCenter.Application.DTOs;

public class PokemonResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public int TreinadorId { get; set; }
    public string? TreinadorNome { get; set; }
}