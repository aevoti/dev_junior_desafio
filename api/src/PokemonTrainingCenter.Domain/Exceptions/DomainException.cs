namespace PokemonTrainingCenter.Domain.Exceptions;

/// <summary>
/// Lançada quando uma regra de negócio (R1-R5) é violada.
/// Mapeada pela Api para HTTP 400 com mensagem amigável ao usuário.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
