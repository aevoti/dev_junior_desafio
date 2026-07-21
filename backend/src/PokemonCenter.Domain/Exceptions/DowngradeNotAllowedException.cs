namespace PokemonCenter.Domain.Exceptions;

public class DowngradeNotAllowedException : DomainException
{
    public DowngradeNotAllowedException()
        : base("Não é permitido fazer downgrade de plano.") { }
}