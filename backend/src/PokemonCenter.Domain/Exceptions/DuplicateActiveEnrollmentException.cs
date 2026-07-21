namespace PokemonCenter.Domain.Exceptions;

public class DuplicateActiveEnrollmentException : DomainException
{
    public DuplicateActiveEnrollmentException()
        : base("Este Pokémon já possui uma matrícula ativa. Cancele-a antes de criar uma nova.") { }
}