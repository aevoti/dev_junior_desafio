namespace PokemonCenter.Domain.Exceptions;

public class EnrollmentNotActiveException : DomainException
{
    public EnrollmentNotActiveException()
        : base("Só é possível fazer upgrade de uma matrícula ativa.") { }
}