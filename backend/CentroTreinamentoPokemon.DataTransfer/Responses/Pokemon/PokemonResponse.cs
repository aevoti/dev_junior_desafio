using CentroTreinamentoPokemon.Domain.Enums;

namespace CentroTreinamentoPokemon.DataTransfer.Responses.Pokemon;

public class PokemonResponse
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public TipoPokemonEnum Tipo { get; set; }

    public int Nivel { get; set; }

    public int TreinadorId { get; set; }

    public string TreinadorNome { get; set; } = string.Empty;
}