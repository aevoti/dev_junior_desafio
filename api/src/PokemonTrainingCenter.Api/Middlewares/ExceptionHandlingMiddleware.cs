using System.Net;
using System.Text.Json;
using PokemonTrainingCenter.Domain.Exceptions;

namespace PokemonTrainingCenter.Api.Middlewares;

/// <summary>
/// Converte DomainException (violação de R1-R5) em HTTP 400 com um payload
/// { message: "..." } que o frontend usa para exibir o erro de forma amigável.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = ex.Message }));
        }
    }
}
