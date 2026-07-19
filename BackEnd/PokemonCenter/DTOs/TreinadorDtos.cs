using System.ComponentModel.DataAnnotations;

namespace PokemonCenter.DTOs;

public record CriarTreinadorRequest(
        [property: Required, StringLength(100)] string Nome,
        [property: Required, EmailAddress, StringLength(150)] string Email,
        [property: Required, StringLength(100)] string CidadeDeOrigem);

public record TreinadorResponse(int Id, string Nome, string Email, string CidadeDeOrigem);
