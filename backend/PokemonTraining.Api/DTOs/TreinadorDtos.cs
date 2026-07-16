using System.ComponentModel.DataAnnotations;

namespace PokemonTraining.Api.DTOs;

public class CriarTreinadorRequest
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string CidadeOrigem { get; set; } = string.Empty;
}

public record TreinadorResponse(int Id, string Nome, string Email, string CidadeOrigem);
