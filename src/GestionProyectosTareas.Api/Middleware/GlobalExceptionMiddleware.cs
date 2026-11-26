using System.Net;
using System.Text.Json;
using GestionProyectosTareas.Application.Exceptions;
using GestionProyectosTareas.Domain.Exceptions;

namespace GestionProyectosTareas.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        (HttpStatusCode statusCode, string title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            ProyectoConTareasPendientesException => (HttpStatusCode.Conflict, "Conflicto de regla de negocio"),
            DomainException => (HttpStatusCode.BadRequest, "Error de validación de dominio"),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Error no controlado en {Path}", context.Request.Path);
        else
            _logger.LogWarning(exception, "Error controlado ({StatusCode}): {Message}", (int)statusCode, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            status = (int)statusCode,
            error = title,
            detail = statusCode == HttpStatusCode.InternalServerError
                ? "Ocurrió un error inesperado al procesar la solicitud."
                : exception.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}