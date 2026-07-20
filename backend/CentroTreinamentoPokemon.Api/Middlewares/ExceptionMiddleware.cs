using CentroTreinamentoPokemon.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CentroTreinamentoPokemon.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionMiddleware> logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (RegraNegocioException exception)
        {
            context.Response.StatusCode =
                (int)HttpStatusCode.BadRequest;

            context.Response.ContentType =
                "application/json";

            string response = JsonSerializer.Serialize(
                new
                {
                    mensagem = exception.Message
                });

            await context.Response.WriteAsync(response);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Erro interno não tratado durante a requisição.");

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType =
                "application/json";

            string response = JsonSerializer.Serialize(
                new
                {
                    mensagem =
                        "Ocorreu um erro interno no servidor."
                });

            await context.Response.WriteAsync(response);
        }
    }
}