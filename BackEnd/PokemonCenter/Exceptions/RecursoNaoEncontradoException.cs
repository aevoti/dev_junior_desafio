namespace PokemonCenter.Exceptions;

public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
