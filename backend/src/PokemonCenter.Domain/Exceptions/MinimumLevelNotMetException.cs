namespace PokemonCenter.Domain.Exceptions;

public class MinimumLevelNotMetException : DomainException
{
    public MinimumLevelNotMetException()
        : base("O Pokémon não possui o nível mínimo necessário para este plano.") { }
}