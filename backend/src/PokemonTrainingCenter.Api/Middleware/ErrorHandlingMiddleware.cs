using System.Text.Json;

namespace PokemonTrainingCenter.Api.Middleware;

/// <summary>
/// Translates domain validation failures and unexpected errors into the
/// API's uniform error shape: <c>{ "message": "..." }</c> in Portuguese,
/// ready for direct display to the end user (contracts/api.md).
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Domain.Exceptions.DomainValidationException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await WriteErrorAsync(context, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteErrorAsync(context, "Ocorreu um erro inesperado. Tente novamente em instantes.");
        }
    }

    private static Task WriteErrorAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { message });
        return context.Response.WriteAsync(payload);
    }
}
