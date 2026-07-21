namespace PokemonCenter.Application.DTOs;

public class MatriculaResponse
{
    public int Id { get; set; }
    public string PokemonNome { get; set; } = string.Empty;
    public string TreinadorNome { get; set; } = string.Empty;
    public string PlanoNome { get; set; } = string.Empty;
    public decimal ValorMensal { get; set; }
    public DateTime DataInicio { get; set; }
    public string Status { get; set; } = string.Empty;
}