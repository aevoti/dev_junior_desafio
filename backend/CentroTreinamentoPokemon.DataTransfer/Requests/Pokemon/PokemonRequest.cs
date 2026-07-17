using CentroTreinamentoPokemon.Domain.Enums;

namespace CentroTreinamentoPokemon.DataTransfer.Requests.Pokemon;

public class PokemonRequest
{
    public string Nome { get; set; } = string.Empty;

    public TipoPokemonEnum Tipo { get; set; }

    public int Nivel { get; set; }

    public int TreinadorId { get; set; }
}