using System.Net;
using System.Text.Json;
using ValyanClinic.Core.Exceptions;

namespace ValyanClinic.Core.Middleware;

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
            _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation failed";
                response.Errors = validationEx.Errors;
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                response.Errors = new List<string> { notFoundEx.Message };
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = unauthorizedEx.Message;
                response.Errors = new List<string> { unauthorizedEx.Message };
                break;

            case BusinessRuleException businessEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = businessEx.Message;
                response.Errors = new List<string> { businessEx.Message };
                break;

            case ConcurrencyException concurrencyEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = concurrencyEx.Message;
                response.Errors = new List<string> { concurrencyEx.Message };
                break;

            case ExternalServiceException serviceEx:
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Message = $"Serviciul {serviceEx.ServiceName} nu este disponibil";
                response.Errors = new List<string> { serviceEx.Message };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "A aparut o eroare interna a serverului";
                response.Errors = new List<string> { "Va rugam sa incercati din nou mai tarziu" };
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
}

// Extension method for easy registration
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}