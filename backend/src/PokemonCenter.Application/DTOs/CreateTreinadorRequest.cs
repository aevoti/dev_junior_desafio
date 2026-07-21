namespace PokemonCenter.Application.DTOs;

public class CreateTreinadorRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CidadeOrigem { get; set; }
}