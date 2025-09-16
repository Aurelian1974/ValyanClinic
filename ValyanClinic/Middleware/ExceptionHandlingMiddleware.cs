using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using ValyanClinic.Application.Exceptions;

namespace ValyanClinic.Middleware;

/// <summary>
/// Middleware pentru gestionarea centralizata a exceptiilor
/// Implementeaza handling specific pentru exceptiile de business
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            BusinessException businessEx => new
            {
                error = businessEx.ErrorCode,
                message = businessEx.Message,
                details = businessEx.Details,
                timestamp = businessEx.Timestamp,
                statusCode = GetStatusCodeFromBusinessException(businessEx),
                additionalInfo = businessEx.ToApiResponse()
            },
            _ => new
            {
                error = "INTERNAL_SERVER_ERROR",
                message = "A aparut o eroare interna",
                details = exception.Message,
                timestamp = DateTime.Now,
                statusCode = (int)HttpStatusCode.InternalServerError,
                additionalInfo = (object?)null
            }
        };

        context.Response.StatusCode = response.statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static int GetStatusCodeFromBusinessException(BusinessException exception)
    {
        return exception switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            ValidationException => (int)HttpStatusCode.BadRequest,
            ConflictException => (int)HttpStatusCode.Conflict,
            UnauthorizedException => (int)HttpStatusCode.Unauthorized,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            ExternalServiceException => (int)HttpStatusCode.ServiceUnavailable,
            BusinessRuleException => (int)HttpStatusCode.UnprocessableEntity,
            ResourceInUseException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}

/// <summary>
/// Extension methods pentru inregistrarea middleware-ului
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Inregistreaza middleware-ul de gestionare exceptii
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
