using System.ComponentModel.DataAnnotations;

namespace PokemonCenter.DTOs;

public record CriarPokemonRequest(
        [property: Required, StringLength(100)] string Nome,
        [property: Required, StringLength(50)] string Tipo,
        [property: Range(1, 100)] int Nivel,
        [property: Range(1, int.MaxValue)] int TreinadorId
        );

public record TransferirPokemonRequest([property: Range(1, int.MaxValue)] int NovoTreinadorId);

public record PokemonResponse(int Id, string Nome, string Tipo, int Nivel, int TreinadorId);
