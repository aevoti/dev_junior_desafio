using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PokemonTraining.Api.Exceptions;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await TratarAsync(context, exception);
        }
    }

    private async Task TratarAsync(HttpContext context, Exception exception)
    {
        var (status, message) = exception switch
        {
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, exception.Message),
            ConflitoException => (StatusCodes.Status409Conflict, exception.Message),
            RegraNegocioException => (StatusCodes.Status400BadRequest, exception.Message),
            DbUpdateException dbException when EhViolacaoDeUnicidade(dbException) =>
                (StatusCodes.Status409Conflict, "Este Pokémon já possui uma matrícula ativa."),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro inesperado ao processar a requisição.");
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { message });
    }

    private static bool EhViolacaoDeUnicidade(DbUpdateException exception)
    {
        var current = exception.InnerException;
        while (current is not null)
        {
            if (current is SqlException sqlException && sqlException.Number is 2601 or 2627)
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }
}
