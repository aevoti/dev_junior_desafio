using System.ComponentModel.DataAnnotations;

namespace PokemonTrainingCenter.DTOs;

public record TreinadorRequest(
    [Required, MaxLength(100)] string Nome,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Required, MaxLength(100)] string CidadeOrigem
);

public record TreinadorResponse(int Id, string Nome, string Email, string CidadeOrigem);
