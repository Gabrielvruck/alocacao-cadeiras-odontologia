using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Intercepta exceções não tratadas e converte para respostas HTTP padronizadas.
    /// </summary>
    /// <param name="context">Contexto HTTP atual.</param>
    public async Task Invoke(HttpContext context)
    {
        var traceId = context.TraceIdentifier;

        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });

            var result = new
            {
                success = false,
                error = new { code = "VALIDATION_ERROR", message = "Erro de validação", details = (string?)null, traceId, action = "Corrija os campos e tente novamente." },
                validationErrors = errors
            };

            // log de aviso para falhas de validação esperadas
            _logger.LogWarning(ex, "Validation failed: {TraceId}", traceId);
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";

            var result = new
            {
                success = false,
                error = new { code = "RESOURCE_NOT_FOUND", message = ex.Message, details = (string?)null, traceId, action = "Verifique o id informado." }
            };

            // recurso não encontrado -> 404
            _logger.LogWarning(ex, "Not found: {TraceId}", traceId);
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = "application/json";

            var result = new
            {
                success = false,
                error = new { code = "BUSINESS_CONFLICT", message = ex.Message, details = (string?)null, traceId, action = "Verifique as regras de negócio." }
            };

            // conflito de regra de negócio -> 409
            _logger.LogWarning(ex, "Business conflict: {TraceId}", traceId);
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var result = new
            {
                success = false,
                error = new { code = "INTERNAL_SERVER_ERROR", message = "Ocorreu um erro interno.", details = (string?)null, traceId, action = "Contate o suporte com o traceId." }
            };

            // erro inesperado -> 500
            _logger.LogError(ex, "Unhandled exception: {TraceId}", traceId);
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
