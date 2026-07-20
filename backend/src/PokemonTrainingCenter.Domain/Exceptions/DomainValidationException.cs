namespace PokemonTrainingCenter.Domain.Exceptions;

/// <summary>
/// Raised by domain services when a business rule is violated (R1-R5).
/// The message is already user-facing Portuguese text (Princípio I/II) —
/// the API layer forwards it verbatim in the error response body.
/// </summary>
public class DomainValidationException : Exception
{
    public int StatusCode { get; }

    public DomainValidationException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
