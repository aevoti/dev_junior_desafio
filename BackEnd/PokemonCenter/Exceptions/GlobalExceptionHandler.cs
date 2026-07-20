using Microsoft.AspNetCore.Diagnostics;

namespace PokemonCenter.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            RegraDeNegocioException => StatusCodes.Status400BadRequest,
            RecursoNaoEncontradoException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var mensagem = statusCode == StatusCodes.Status500InternalServerError
            ? "Ocorreu um erro inesperado"
            : exception.Message;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new { mensagem }, cancellationToken);

        return true;
    }
}
